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
    }

    public void Save(AdoptionListing listing)
    {
        dbContext.Listings.Update(listing);
    }

    public void Delete(AdoptionListing listing)
    {
        dbContext.Listings.Remove(listing);
    }
}
