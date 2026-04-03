using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Application.Queries;
using PetZone.Listings.Contracts;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Infrastructure.Queries;

public class GetMyListingsHandler(ListingsDbContext dbContext)
{
    public async Task<IReadOnlyList<ListingDto>> Handle(
        GetMyListingsQuery query,
        CancellationToken ct = default)
    {
        var items = await dbContext.Listings
            .Where(l => l.UserId == query.UserId && l.Status != ListingStatus.Removed)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return items.Select(l => new ListingDto(
            l.Id, l.UserId, l.UserName, l.UserEmail, l.UserPhone, l.ContactEmail,
            l.Title, l.Description, l.SpeciesId, l.BreedId,
            l.AgeMonths, l.Color, l.City, l.Vaccinated, l.Castrated,
            l.Photos, l.Status.ToString(), l.CreatedAt)).ToList();
    }
}
