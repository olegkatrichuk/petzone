using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Application.Events;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Consumers;

public class VolunteerRequestStatusChangedConsumer(
    INotificationDbContext dbContext,
    ILogger<VolunteerRequestStatusChangedConsumer> logger)
    : IConsumer<VolunteerRequestStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<VolunteerRequestStatusChangedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received VolunteerRequestStatusChanged for user {UserId}, status: {Status}",
            message.UserId, message.Status);

        var settings = await dbContext.UserNotificationSettings
            .FirstOrDefaultAsync(s => s.UserId == message.UserId);

        if (settings is null)
        {
            logger.LogWarning("No notification settings found for user {UserId}", message.UserId);
            return;
        }

        if (settings.SendEmail)
        {
            logger.LogInformation("Sending email notification to user {UserId}", message.UserId);
            // TODO: реальна відправка email
        }

        if (settings.SendTelegram)
        {
            logger.LogInformation("Sending telegram notification to user {UserId}", message.UserId);
            // TODO: реальна відправка telegram
        }

        if (settings.SendWeb)
        {
            logger.LogInformation("Sending web notification to user {UserId}", message.UserId);
            // TODO: реальна відправка web
        }
    }
}