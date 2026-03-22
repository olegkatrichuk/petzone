using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetZone.Volunteers.Infrastructure.Options;

namespace PetZone.Volunteers.Infrastructure.BackgroundServices;

public class SoftDeleteCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<SoftDeleteCleanupService> logger,
    IOptions<SoftDeleteOptions> options) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SoftDeleteCleanupService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);

            logger.LogInformation("Next cleanup in {Hours} hours", _interval.TotalHours);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting soft delete cleanup");

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();

        var retentionDays = options.Value.RetentionDays;
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        try
        {
            var volunteersToDelete = await dbContext.Volunteers
                .Where(v => v.IsDeleted && v.DeletedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (volunteersToDelete.Count == 0)
            {
                logger.LogInformation("No expired soft-deleted volunteers found");
                return;
            }

            dbContext.Volunteers.RemoveRange(volunteersToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Hard deleted {Count} expired volunteers (older than {Days} days)",
                volunteersToDelete.Count, retentionDays);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during soft delete cleanup");
        }
    }
}