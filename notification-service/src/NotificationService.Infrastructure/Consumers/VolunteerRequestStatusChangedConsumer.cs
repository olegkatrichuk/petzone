using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Infrastructure.Email;
using PetZone.VolunteerRequests.Application.Events;
using System.Net.Http.Json;

namespace NotificationService.Infrastructure.Consumers;

public class VolunteerRequestStatusChangedConsumer(
    INotificationDbContext dbContext,
    IEmailSender emailSender,
    IHttpClientFactory httpClientFactory,
    ILogger<VolunteerRequestStatusChangedConsumer> logger)
    : IConsumer<VolunteerRequestStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<VolunteerRequestStatusChangedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received VolunteerRequestStatusChanged for user {UserId}, status: {Status}",
            message.UserId, message.Status);

        var settings = await dbContext.UserNotificationSettings
            .FirstOrDefaultAsync(s => s.UserId == message.UserId, context.CancellationToken);

        var sendEmail = settings?.SendEmail ?? true;

        if (!sendEmail)
            return;

        try
        {
            var client = httpClientFactory.CreateClient("AccountsApi");
            var response = await client.GetAsync(
                $"accounts/{message.UserId}", context.CancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get user info for {UserId}", message.UserId);
                return;
            }

            var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>(
                cancellationToken: context.CancellationToken);

            if (userInfo is null)
            {
                logger.LogError("User info is null for {UserId}", message.UserId);
                return;
            }

            var (subject, body) = BuildEmail(message, userInfo);

            await emailSender.SendAsync(
                userInfo.Email,
                subject,
                body,
                context.CancellationToken);

            logger.LogInformation(
                "Email notification sent to {Email} for status {Status}",
                userInfo.Email, message.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send notification for user {UserId}", message.UserId);
        }
    }

    private static (string Subject, string Body) BuildEmail(
        VolunteerRequestStatusChangedEvent message,
        UserInfo user)
    {
        return message.Status switch
        {
            "Approved" => (
                "Вашу заявку волонтера схвалено — PetZone",
                $"""
                <h2>Вітаємо, {user.FirstName}!</h2>
                <p>Ваша заявка волонтера була розглянута та <strong>схвалена</strong>.</p>
                <p>Тепер ви є волонтером PetZone. Дякуємо за вашу готовність допомагати тваринам!</p>
                """),
            "Rejected" => (
                "Заявку волонтера відхилено — PetZone",
                $"""
                <h2>Шановний(а) {user.FirstName},</h2>
                <p>На жаль, вашу заявку волонтера було <strong>відхилено</strong>.</p>
                {(string.IsNullOrEmpty(message.Comment) ? "" : $"<p>Причина: {message.Comment}</p>")}
                <p>Якщо у вас є запитання, будь ласка, зв'яжіться з нами.</p>
                """),
            "RevisionRequired" => (
                "Заявка волонтера потребує доопрацювання — PetZone",
                $"""
                <h2>Шановний(а) {user.FirstName},</h2>
                <p>Вашу заявку волонтера <strong>відправлено на доопрацювання</strong>.</p>
                {(string.IsNullOrEmpty(message.Comment) ? "" : $"<p>Коментар адміністратора: {message.Comment}</p>")}
                <p>Будь ласка, внесіть необхідні зміни та повторно подайте заявку.</p>
                """),
            _ => (
                "Статус вашої заявки змінився — PetZone",
                $"<p>Статус вашої заявки змінено на: {message.Status}</p>")
        };
    }

    private record UserInfo(Guid Id, string Email, string FirstName, string LastName);
}
