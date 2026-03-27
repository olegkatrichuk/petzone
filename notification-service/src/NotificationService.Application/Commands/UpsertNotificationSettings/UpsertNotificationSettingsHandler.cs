using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Application.Commands.UpsertNotificationSettings;

public class UpsertNotificationSettingsHandler(
    INotificationDbContext dbContext)
{
    public async Task<Result<Guid, string>> Handle(
        UpsertNotificationSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserNotificationSettings
            .FirstOrDefaultAsync(s => s.UserId == command.UserId, cancellationToken);

        if (existing is null)
        {
            var settings = UserNotificationSettings.Create(
                command.UserId,
                command.SendEmail ?? true,
                command.SendTelegram ?? false,
                command.SendWeb ?? true);

            await dbContext.UserNotificationSettings.AddAsync(settings, cancellationToken);
        }
        else
        {
            existing.Update(command.SendEmail, command.SendTelegram, command.SendWeb);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return existing?.Id ?? dbContext.UserNotificationSettings
            .First(s => s.UserId == command.UserId).Id;
    }
}