using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetZone.VolunteerRequests.Infrastructure;

public class VolunteerRequestsDbContextFactory
    : IDesignTimeDbContextFactory<VolunteerRequestsDbContext>
{
    public VolunteerRequestsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VolunteerRequestsDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5434;Database=petzone_db;Username=petzone;Password=Ruslan2802@",
            b => b.MigrationsHistoryTable("__EFMigrationsHistory", "volunteer_requests"));

        return new VolunteerRequestsDbContext(optionsBuilder.Options);
    }
}