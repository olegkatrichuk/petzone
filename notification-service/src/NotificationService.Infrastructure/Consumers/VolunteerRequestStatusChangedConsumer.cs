using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Infrastructure.Email;
using PetZone.VolunteerRequests.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class VolunteerRequestStatusChangedConsumer(
    INotificationDbContext dbContext,
    IEmailSender emailSender,
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

        if (settings?.SendEmail == false)
            return;

        if (string.IsNullOrEmpty(message.Email))
        {
            logger.LogError("Email is empty for user {UserId}, skipping notification", message.UserId);
            return;
        }

        try
        {
            var (subject, body) = BuildEmail(message);

            await emailSender.SendAsync(
                message.Email,
                subject,
                body,
                context.CancellationToken);

            logger.LogInformation(
                "Email notification sent to {Email} for status {Status}",
                message.Email, message.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send notification for user {UserId}", message.UserId);
        }
    }

    private static (string Subject, string Body) BuildEmail(VolunteerRequestStatusChangedEvent message)
    {
        return message.Status switch
        {
            "Approved" => (
                "Вашу заявку волонтера схвалено — PetZone",
                $"""
                <h2>Вітаємо, {message.FirstName}!</h2>
                <p>Ваша заявка волонтера була розглянута та <strong>схвалена</strong>.</p>
                <p>Тепер ви є волонтером PetZone. Дякуємо за вашу готовність допомагати тваринам!</p>
                """),
            "Rejected" => (
                "Заявку волонтера відхилено — PetZone",
                $"""
                <h2>Шановний(а) {message.FirstName},</h2>
                <p>На жаль, вашу заявку волонтера було <strong>відхилено</strong>.</p>
                {(string.IsNullOrEmpty(message.Comment) ? "" : $"<p>Причина: {message.Comment}</p>")}
                <p>Якщо у вас є запитання, будь ласка, зв'яжіться з нами.</p>
                """),
            "RevisionRequired" => (
                "Заявка волонтера потребує доопрацювання — PetZone",
                $"""
                <h2>Шановний(а) {message.FirstName},</h2>
                <p>Вашу заявку волонтера <strong>відправлено на доопрацювання</strong>.</p>
                {(string.IsNullOrEmpty(message.Comment) ? "" : $"<p>Коментар адміністратора: {message.Comment}</p>")}
                <p>Будь ласка, внесіть необхідні зміни та повторно подайте заявку.</p>
                """),
            _ => (
                "Статус вашої заявки змінився — PetZone",
                $"<p>Статус вашої заявки змінено на: {message.Status}</p>")
        };
    }
}
