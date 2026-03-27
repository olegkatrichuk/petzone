using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Application;

public interface INotificationDbContext
{
    DbSet<UserNotificationSettings> UserNotificationSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}