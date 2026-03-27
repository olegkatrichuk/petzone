namespace NotificationService.Application.Commands.UpsertNotificationSettings;

public record UpsertNotificationSettingsCommand(
    Guid UserId,
    bool? SendEmail,
    bool? SendTelegram,
    bool? SendWeb
);