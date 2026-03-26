using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetZone.VolunteerRequests.Infrastructure;

public class VolunteerRequestsDbContextFactory
    : IDesignTimeDbContextFactory<VolunteerRequestsDbContext>
{
    public VolunteerRequestsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5434;Database=petzone_db;Username=petzone;Password=changeme";

        var optionsBuilder = new DbContextOptionsBuilder<VolunteerRequestsDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            b => b.MigrationsHistoryTable("__EFMigrationsHistory", "volunteer_requests"));

        return new VolunteerRequestsDbContext(optionsBuilder.Options);
    }
}