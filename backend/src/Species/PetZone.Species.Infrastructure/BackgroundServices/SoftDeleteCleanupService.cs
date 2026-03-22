using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetZone.Species.Infrastructure.Options;

namespace PetZone.Species.Infrastructure.BackgroundServices;

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
        logger.LogInformation("Starting species soft delete cleanup");

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();

        var retentionDays = options.Value.RetentionDays;
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        try
        {
            // Species does not currently implement soft delete
            // This service is a placeholder for future use
            await Task.CompletedTask;

            logger.LogInformation("Species soft delete cleanup completed (no-op)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during species soft delete cleanup");
        }
    }
}