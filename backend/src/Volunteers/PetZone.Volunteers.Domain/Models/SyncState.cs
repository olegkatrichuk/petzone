namespace PetZone.Volunteers.Domain.Models;

// Persisted timestamp of the last successful sync per external-data source.
// Used by sync hosted services to skip work when restarted within their
// 24h cycle — otherwise every redeploy re-fetches everything and the
// pet count grows visibly even though dedup by ExternalId prevents true
// duplicates (new external listings are real, but redeploy compresses
// what would be a once-a-day intake into "on every push").
public class SyncState
{
    public const int MaxServiceNameLength = 64;

    public Guid Id { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public DateTime LastRunAt { get; private set; }

    private SyncState() { }

    public static SyncState Create(string serviceName, DateTime utc) =>
        new()
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            LastRunAt = DateTime.SpecifyKind(utc, DateTimeKind.Utc),
        };

    public void RecordRun(DateTime utc) =>
        LastRunAt = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
}
