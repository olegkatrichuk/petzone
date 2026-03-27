using Microsoft.EntityFrameworkCore;
using NotificationService.Application;
using NotificationService.Domain;

namespace NotificationService.Infrastructure;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options), INotificationDbContext
{
    public DbSet<UserNotificationSettings> UserNotificationSettings => Set<UserNotificationSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}