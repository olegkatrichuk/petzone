using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure;

public class NotificationDbContextFactory
    : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5434;Database=petzone_db;Username=petzone;Password=Ruslan2802@",
            b => b.MigrationsHistoryTable("__EFMigrationsHistory", "notifications"));

        return new NotificationDbContext(optionsBuilder.Options);
    }
}