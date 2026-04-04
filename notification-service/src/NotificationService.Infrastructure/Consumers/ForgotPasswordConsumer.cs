using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Email;
using PetZone.Accounts.Application.Events;

namespace NotificationService.Infrastructure.Consumers;

public class ForgotPasswordConsumer(
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<ForgotPasswordConsumer> logger)
    : IConsumer<ForgotPasswordEvent>
{
    public async Task Consume(ConsumeContext<ForgotPasswordEvent> context)
    {
        var message = context.Message;

        logger.LogInformation("Received ForgotPassword event for {Email}", message.Email);

        try
        {
            var encodedToken = Uri.EscapeDataString(message.ResetToken);
            var siteUrl = configuration["SiteUrl"] ?? "https://getpetzone.com";
            var resetLink = $"{siteUrl}/reset-password?userId={message.UserId}&token={encodedToken}";

            var body = $"""
                <h2>Скидання паролю — PetZone</h2>
                <p>Привіт, {message.FirstName}!</p>
                <p>Ми отримали запит на скидання вашого паролю. Перейдіть за посиланням нижче:</p>
                <p><a href="{resetLink}">Скинути пароль</a></p>
                <p>Посилання дійсне протягом 24 годин.</p>
                <p>Якщо ви не надсилали цей запит — проігноруйте лист. Ваш пароль залишається незмінним.</p>
                """;

            await emailSender.SendAsync(
                message.Email,
                "Скидання паролю — PetZone",
                body,
                context.CancellationToken);

            logger.LogInformation("Password reset email sent to {Email}", message.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", message.Email);
        }
    }
}