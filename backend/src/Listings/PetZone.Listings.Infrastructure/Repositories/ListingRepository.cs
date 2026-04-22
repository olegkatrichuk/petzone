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

    public async Task<bool> ActiveListingExistsAsync(Guid userId, string title, CancellationToken ct = default)
        => await dbContext.Listings.AnyAsync(
            l => l.UserId == userId &&
                 l.Status == ListingStatus.Active &&
                 EF.Functions.ILike(l.Title, title),
            ct);
}
