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
                "Set the 'ConnectionStrings__Database' environment variable before running migrations.");

        var optionsBuilder = new DbContextOptionsBuilder<VolunteerRequestsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new VolunteerRequestsDbContext(optionsBuilder.Options);
    }
}