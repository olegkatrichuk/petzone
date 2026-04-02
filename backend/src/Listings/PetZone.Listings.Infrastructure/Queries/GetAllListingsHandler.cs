using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Application.Queries;
using PetZone.Listings.Contracts;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Infrastructure.Queries;

public class GetAllListingsHandler(ListingsDbContext dbContext)
{
    public async Task<IReadOnlyList<ListingDto>> Handle(
        GetAllListingsQuery query,
        CancellationToken ct = default)
    {
        var q = dbContext.Listings
            .Where(l => l.Status == ListingStatus.Active)
            .AsQueryable();

        if (query.SpeciesId.HasValue)
            q = q.Where(l => l.SpeciesId == query.SpeciesId.Value);

        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(l => l.City.ToLower().Contains(query.City.ToLower()));

        var items = await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return items.Select(ToDto).ToList();
    }

    private static ListingDto ToDto(AdoptionListing l) => new(
        l.Id, l.UserId, l.UserName, l.UserEmail, l.UserPhone,
        l.Title, l.Description, l.SpeciesId, l.BreedId,
        l.AgeMonths, l.Color, l.City, l.Vaccinated, l.Castrated,
        l.Photos, l.Status.ToString(), l.CreatedAt);
}
