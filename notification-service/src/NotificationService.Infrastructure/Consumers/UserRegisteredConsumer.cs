using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Accounts.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class UserRegisteredConsumer(
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<UserRegisteredConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Received UserRegistered event for {Email}", message.Email);

        try
        {
            var encodedToken = Uri.EscapeDataString(message.ConfirmationToken);
            var siteUrl = configuration["SiteUrl"] ?? "https://getpetzone.com";
            var confirmationLink = $"{siteUrl}/confirm-email?userId={message.UserId}&token={encodedToken}";

            var body = $"""
                <h2>Ласкаво просимо до PetZone, {message.FirstName}!</h2>
                <p>Для підтвердження вашої електронної пошти перейдіть за посиланням:</p>
                <a href="{confirmationLink}">Підтвердити пошту</a>
                <p>Якщо ви не реєструвались на PetZone, проігноруйте цей лист.</p>
                """;

            await emailSender.SendAsync(
                message.Email,
                "Підтвердіть вашу пошту - PetZone",
                body,
                context.CancellationToken);

            logger.LogInformation("Confirmation email sent to {Email}", message.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send confirmation email to {Email}", message.Email);
        }
    }
}