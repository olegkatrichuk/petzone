using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Volunteers.Application.Events; // local copy in Events/AdoptionApplicationCreatedEvent.cs

namespace NotificationService.Infrastructure.Consumers;

public class AdoptionApplicationCreatedConsumer(
    IEmailSender emailSender,
    ILogger<AdoptionApplicationCreatedConsumer> logger)
    : IConsumer<AdoptionApplicationCreatedEvent>
{
    public async Task Consume(ConsumeContext<AdoptionApplicationCreatedEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Received AdoptionApplicationCreated for pet {PetId}", msg.PetId);

        try
        {
            var messageBlock = string.IsNullOrWhiteSpace(msg.Message)
                ? ""
                : $"<p><strong>Повідомлення від заявника:</strong><br/>{msg.Message}</p>";

            var body = $"""
                <h2>Нова заявка на усиновлення — PetZone</h2>
                <p>Доброго дня, {msg.VolunteerName}!</p>
                <p>Користувач <strong>{msg.ApplicantName}</strong> хоче усиновити вашого вихованця <strong>«{msg.PetNickname}»</strong>.</p>
                <p><strong>Контактний телефон:</strong> {msg.ApplicantPhone}</p>
                {messageBlock}
                <p>Перейдіть до свого кабінету на PetZone, щоб переглянути заявку та прийняти рішення.</p>
                <br/>
                <p>З турботою,<br/>Команда PetZone 🐾</p>
                """;

            await emailSender.SendAsync(
                msg.VolunteerEmail,
                $"Нова заявка на «{msg.PetNickname}» — PetZone",
                body,
                context.CancellationToken);

            logger.LogInformation("Adoption application email sent to {Email}", msg.VolunteerEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send adoption application email to {Email}", msg.VolunteerEmail);
        }
    }
}
