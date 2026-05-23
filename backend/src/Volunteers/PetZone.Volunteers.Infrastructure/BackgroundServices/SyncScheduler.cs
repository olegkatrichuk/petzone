using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.BackgroundServices;

// Shared helper for hosted sync services: returns how long to wait before
// the next run, based on a persisted LastRunAt. Stops redeploy-triggered
// re-runs from inflating data — sync only fires once every `interval`
// regardless of how often the container restarts.
public static class SyncScheduler
{
    /// <summary>
    /// Returns the delay before the next sync should run. If LastRunAt is
    /// null (first ever run), returns `initialDelay`. Otherwise returns
    /// `max(initialDelay, lastRun + interval - now)` so the service always
    /// waits out the remaining 24h window after a restart.
    /// </summary>
    public static async Task<TimeSpan> ComputeDelayAsync(
        IServiceProvider serviceProvider,
        string serviceName,
        TimeSpan interval,
        TimeSpan initialDelay,
        ILogger logger,
        CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();

        var state = await db.SyncStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceName == serviceName, ct);

        if (state is null)
        {
            logger.LogInformation("[{Service}] no prior run recorded — waiting initial delay {Delay}",
                serviceName, initialDelay);
            return initialDelay;
        }

        var due = state.LastRunAt + interval;
        var now = DateTime.UtcNow;
        if (due <= now)
        {
            logger.LogInformation("[{Service}] last run {LastRun:O} is older than {Interval} — waiting only initial delay {Delay}",
                serviceName, state.LastRunAt, interval, initialDelay);
            return initialDelay;
        }

        var wait = due - now;
        if (wait < initialDelay) wait = initialDelay;
        logger.LogInformation("[{Service}] last run {LastRun:O}, next due in {Wait}",
            serviceName, state.LastRunAt, wait);
        return wait;
    }

    /// <summary>
    /// Upserts the LastRunAt timestamp for the given service to UtcNow.
    /// </summary>
    public static async Task RecordRunAsync(
        IServiceProvider serviceProvider,
        string serviceName,
        CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();

        var state = await db.SyncStates.FirstOrDefaultAsync(s => s.ServiceName == serviceName, ct);
        var now = DateTime.UtcNow;
        if (state is null)
        {
            db.SyncStates.Add(SyncState.Create(serviceName, now));
        }
        else
        {
            state.RecordRun(now);
        }
        await db.SaveChangesAsync(ct);
    }
}
