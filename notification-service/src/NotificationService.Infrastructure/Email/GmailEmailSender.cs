using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace NotificationService.Infrastructure.Email;

public class GmailEmailSender(
    IConfiguration configuration,
    ILogger<GmailEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            configuration["Email:SenderName"] ?? "PetZone",
            configuration["Email:SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
            int.Parse(configuration["Email:SmtpPort"] ?? "587"),
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            configuration["Email:SenderEmail"],
            configuration["Email:SmtpPassword"],
            cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Email sent to {To}", to);
    }
}