using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Infrastructure;

public class ListingsDbContext(DbContextOptions<ListingsDbContext> options) : DbContext(options)
{
    public DbSet<AdoptionListing> Listings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListingsDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveMaxLength(500);
    }
}
