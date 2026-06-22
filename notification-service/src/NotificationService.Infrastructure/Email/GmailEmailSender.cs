using System.Net;
using System.Text.RegularExpressions;
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
        message.Body = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = HtmlToPlainText(body),
        }.ToMessageBody();

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

    // Minimal HTML→text so the email carries a plain-text alternative.
    // Links must survive, so <a href="url">text</a> becomes "text: url".
    private static string HtmlToPlainText(string html)
    {
        var text = Regex.Replace(
            html,
            @"<a\s[^>]*href=""([^""]*)""[^>]*>(.*?)</a>",
            "$2: $1",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        text = Regex.Replace(text, @"<[^>]+>", string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"[ \t]*\n\s*\n\s*", "\n\n");

        return text.Trim();
    }
}