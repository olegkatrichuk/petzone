using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Application.Providers;

namespace PetZone.Volunteers.Infrastructure.BackgroundServices;

public class MinioCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<MinioCleanupService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);
    private readonly TimeSpan _fileRetention = TimeSpan.FromHours(1);
    private const string BucketName = "petzone";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MinioCleanupService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);

            logger.LogInformation("Next Minio cleanup in {Hours} hours", _interval.TotalHours);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Minio cleanup");

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        var filesProvider = scope.ServiceProvider.GetRequiredService<IFilesProvider>();

        try
        {
            var listResult = await filesProvider.ListFiles(BucketName, cancellationToken);
            if (listResult.IsFailure)
            {
                logger.LogError("Failed to list Minio files: {Error}", listResult.Error.Description);
                return;
            }

            var minioFiles = listResult.Value;

            var dbPhotoPaths = await dbContext.Volunteers
                .SelectMany(v => v.Pets.SelectMany(p => p.Photos.Select(ph => ph.FilePath)))
                .ToListAsync(cancellationToken);

            var dbPathsSet = dbPhotoPaths.ToHashSet();

            var cutoffTime = DateTime.UtcNow - _fileRetention;

            var orphanFiles = minioFiles
                .Where(f => !dbPathsSet.Contains(f.FileName) && f.LastModified < cutoffTime)
                .ToList();

            if (orphanFiles.Count == 0)
            {
                logger.LogInformation("No orphan files found in Minio");
                return;
            }

            foreach (var file in orphanFiles)
            {
                var deleteResult = await filesProvider.DeleteFile(BucketName, file.FileName, cancellationToken);
                if (deleteResult.IsFailure)
                {
                    logger.LogWarning("Failed to delete orphan file {FileName}: {Error}",
                        file.FileName, deleteResult.Error.Description);
                }
                else
                {
                    logger.LogInformation("Deleted orphan file {FileName}", file.FileName);
                }
            }

            logger.LogInformation("Minio cleanup completed. Deleted {Count} orphan files", orphanFiles.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Minio cleanup");
        }
    }
}