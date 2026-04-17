using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Application.Queries;
using PetZone.Listings.Contracts;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Infrastructure.Queries;

public class GetAllListingsHandler(ListingsDbContext dbContext)
{
    public async Task<PagedListingsResult> Handle(
        GetAllListingsQuery query,
        CancellationToken ct = default)
    {
        var q = dbContext.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active)
            .AsQueryable();

        if (query.SpeciesId.HasValue)
            q = q.Where(l => l.SpeciesId == query.SpeciesId.Value);

        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(l => EF.Functions.ILike(l.City, $"%{query.City}%"));

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(l =>
                EF.Functions.ILike(l.Title, $"%{query.Search}%") ||
                EF.Functions.ILike(l.Description, $"%{query.Search}%"));

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new ListingDto(
                l.Id, l.UserId, l.UserName, l.UserEmail, l.UserPhone, l.ContactEmail,
                l.Title, l.Description, l.SpeciesId, l.BreedId,
                l.AgeMonths, l.Color, l.City, l.Vaccinated, l.Castrated,
                l.Photos, l.Status.ToString(), l.CreatedAt))
            .ToListAsync(ct);

        return new PagedListingsResult(items, totalCount, query.Page, query.PageSize);
    }
}
