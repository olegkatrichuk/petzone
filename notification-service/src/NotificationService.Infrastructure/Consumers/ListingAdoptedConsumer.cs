using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Listings.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class ListingAdoptedConsumer(
    IEmailSender emailSender,
    ILogger<ListingAdoptedConsumer> logger)
    : IConsumer<ListingAdoptedEvent>
{
    public async Task Consume(ConsumeContext<ListingAdoptedEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Received ListingAdopted event for listing {ListingId}", msg.ListingId);

        try
        {
            var body = $"""
                <h2>🎉 Вітаємо, {msg.UserName}!</h2>
                <p>Чудова новина — ваш вихованець з оголошення <strong>«{msg.Title}»</strong> знайшов нову сім'ю!</p>
                <p>Оголошення закрито. Дякуємо, що скористались PetZone для пошуку дому для вашого улюбленця.</p>
                <br/>
                <p>Якщо у вас є ще тварини, яким потрібна нова сім'я — ми завжди раді допомогти.</p>
                <br/>
                <p>З теплом,<br/>Команда PetZone 🐾</p>
                """;

            await emailSender.SendAsync(
                msg.UserEmail,
                "Вашого вихованця знайшла нова сім'я! — PetZone",
                body,
                context.CancellationToken);

            logger.LogInformation("Listing adopted email sent to {Email}", msg.UserEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send listing adopted email to {Email}", msg.UserEmail);
        }
    }
}