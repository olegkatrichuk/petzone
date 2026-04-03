using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using PetZone.Listings.Application.Queries;
using PetZone.Listings.Contracts;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Infrastructure.Queries;

public class GetListingByIdHandler(ListingsDbContext dbContext)
{
    public async Task<Result<ListingDto, Error>> Handle(
        GetListingByIdQuery query,
        CancellationToken ct = default)
    {
        var listing = await dbContext.Listings
            .FirstOrDefaultAsync(l => l.Id == query.ListingId, ct);

        if (listing is null)
            return Error.NotFound("listing.not_found", "Оголошення не знайдено");

        return ToDto(listing);
    }

    private static ListingDto ToDto(AdoptionListing l) => new(
        l.Id, l.UserId, l.UserName, l.UserEmail, l.UserPhone, l.ContactEmail,
        l.Title, l.Description, l.SpeciesId, l.BreedId,
        l.AgeMonths, l.Color, l.City, l.Vaccinated, l.Castrated,
        l.Photos, l.Status.ToString(), l.CreatedAt);
}
