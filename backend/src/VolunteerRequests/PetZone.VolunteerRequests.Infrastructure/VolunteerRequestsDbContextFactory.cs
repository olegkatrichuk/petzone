using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetZone.VolunteerRequests.Infrastructure;

public class VolunteerRequestsDbContextFactory
    : IDesignTimeDbContextFactory<VolunteerRequestsDbContext>
{
    public VolunteerRequestsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__Database environment variable is required for migrations. " +
                "Set it before running 'dotnet ef' commands.");

        var optionsBuilder = new DbContextOptionsBuilder<VolunteerRequestsDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            b => b.MigrationsHistoryTable("__EFMigrationsHistory", "volunteer_requests"));

        return new VolunteerRequestsDbContext(optionsBuilder.Options);
    }
}