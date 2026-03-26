using Microsoft.EntityFrameworkCore;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure;

public class VolunteerRequestsDbContext(DbContextOptions<VolunteerRequestsDbContext> options)
    : DbContext(options)
{
    public DbSet<VolunteerRequest> VolunteerRequests => Set<VolunteerRequest>();
    public DbSet<RejectedUser> RejectedUsers => Set<RejectedUser>();
    public DbSet<Discussion> Discussions => Set<Discussion>(); // ← додай

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("volunteer_requests");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VolunteerRequestsDbContext).Assembly);
    }
}