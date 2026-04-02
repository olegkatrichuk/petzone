using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Listings.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class ListingCreatedConsumer(
    IEmailSender emailSender,
    ILogger<ListingCreatedConsumer> logger)
    : IConsumer<ListingCreatedEvent>
{
    public async Task Consume(ConsumeContext<ListingCreatedEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Received ListingCreated event for listing {ListingId}", msg.ListingId);

        try
        {
            var body = $"""
                <h2>Вітаємо, {msg.UserName}!</h2>
                <p>Ваше оголошення <strong>«{msg.Title}»</strong> успішно опубліковано на PetZone.</p>
                <p>Місто: {msg.City}</p>
                <p>Воно вже доступне для перегляду всіма відвідувачами сайту.</p>
                <br/>
                <p>Коли знайдете нову сім'ю для вашого вихованця — не забудьте відмітити оголошення як «Знайшов дім» у своєму кабінеті.</p>
                <br/>
                <p>З турботою,<br/>Команда PetZone 🐾</p>
                """;

            await emailSender.SendAsync(
                msg.UserEmail,
                "Ваше оголошення опубліковано — PetZone",
                body,
                context.CancellationToken);

            logger.LogInformation("Listing created email sent to {Email}", msg.UserEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send listing created email to {Email}", msg.UserEmail);
        }
    }
}