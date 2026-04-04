using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Accounts.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class UserRegisteredConsumer(
    IEmailSender emailSender,
    IHttpClientFactory httpClientFactory,
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
            var client = httpClientFactory.CreateClient("AccountsApi");

            var response = await client.GetAsync(
                $"accounts/{message.UserId}/confirmation-token");

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(token))
            {
                logger.LogError("Failed to get confirmation token for user {UserId}", message.UserId);
                return;
            }

            var encodedToken = Uri.EscapeDataString(token);
            var publicApiUrl = configuration["SiteUrl"] ?? "https://getpetzone.com";
            var confirmationLink = $"{publicApiUrl}/confirm-email?userId={message.UserId}&token={encodedToken}";

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