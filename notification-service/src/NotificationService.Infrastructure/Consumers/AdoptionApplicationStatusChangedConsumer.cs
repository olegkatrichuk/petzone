using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Volunteers.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class AdoptionApplicationStatusChangedConsumer(
    IEmailSender emailSender,
    ILogger<AdoptionApplicationStatusChangedConsumer> logger)
    : IConsumer<AdoptionApplicationStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<AdoptionApplicationStatusChangedEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("AdoptionApplicationStatusChanged: {Status} for pet {PetId}", msg.Status, msg.PetId);

        try
        {
            var (subject, body) = msg.Status == "Approved"
                ? BuildApprovedEmail(msg)
                : BuildRejectedEmail(msg);

            await emailSender.SendAsync(
                msg.ApplicantEmail,
                subject,
                body,
                context.CancellationToken);

            logger.LogInformation("Status changed email sent to {Email} ({Status})", msg.ApplicantEmail, msg.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send status changed email to {Email}", msg.ApplicantEmail);
        }
    }

    private static (string subject, string body) BuildApprovedEmail(AdoptionApplicationStatusChangedEvent msg)
    {
        var subject = $"Вашу заявку на «{msg.PetNickname}» схвалено — PetZone";
        var body = $"""
            <h2>Вітаємо! Ваша заявка схвалена 🎉</h2>
            <p>Доброго дня, {msg.ApplicantName}!</p>
            <p>Волонтер <strong>{msg.VolunteerName}</strong> схвалив вашу заявку на усиновлення <strong>«{msg.PetNickname}»</strong>.</p>
            <p>Зв'яжіться з волонтером, щоб узгодити деталі передачі тварини.</p>
            <br/>
            <p>Дякуємо, що даєте тварині новий дім! 🐾</p>
            <p>З турботою,<br/>Команда PetZone</p>
            """;
        return (subject, body);
    }

    private static (string subject, string body) BuildRejectedEmail(AdoptionApplicationStatusChangedEvent msg)
    {
        var subject = $"Статус вашої заявки на «{msg.PetNickname}» — PetZone";
        var body = $"""
            <h2>Статус заявки змінено</h2>
            <p>Доброго дня, {msg.ApplicantName}!</p>
            <p>На жаль, волонтер <strong>{msg.VolunteerName}</strong> не зміг прийняти вашу заявку на <strong>«{msg.PetNickname}»</strong> цього разу.</p>
            <p>Не засмучуйтесь — на PetZone тисячі тварин шукають дім. Можливо, ваш новий друг вже чекає на вас!</p>
            <br/>
            <p>З турботою,<br/>Команда PetZone 🐾</p>
            """;
        return (subject, body);
    }
}