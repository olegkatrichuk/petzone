using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Infrastructure.Repositories;

public class ListingRepository(ListingsDbContext dbContext) : IListingRepository
{
    public async Task<AdoptionListing?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Listings.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task AddAsync(AdoptionListing listing, CancellationToken ct = default)
    {
        await dbContext.Listings.AddAsync(listing, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(AdoptionListing listing, CancellationToken ct = default)
    {
        dbContext.Listings.Update(listing);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(AdoptionListing listing, CancellationToken ct = default)
    {
        dbContext.Listings.Remove(listing);
        await dbContext.SaveChangesAsync(ct);
    }
}
