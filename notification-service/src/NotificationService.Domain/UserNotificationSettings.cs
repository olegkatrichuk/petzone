using CSharpFunctionalExtensions;

namespace NotificationService.Domain;

public class UserNotificationSettings
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public bool SendEmail { get; private set; }
    public bool SendTelegram { get; private set; }
    public bool SendWeb { get; private set; }

    private UserNotificationSettings() { }

    public static UserNotificationSettings Create(
        Guid userId,
        bool sendEmail = true,
        bool sendTelegram = false,
        bool sendWeb = true)
    {
        return new UserNotificationSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SendEmail = sendEmail,
            SendTelegram = sendTelegram,
            SendWeb = sendWeb
        };
    }

    public void Update(bool? sendEmail, bool? sendTelegram, bool? sendWeb)
    {
        if (sendEmail.HasValue) SendEmail = sendEmail.Value;
        if (sendTelegram.HasValue) SendTelegram = sendTelegram.Value;
        if (sendWeb.HasValue) SendWeb = sendWeb.Value;
    }
}