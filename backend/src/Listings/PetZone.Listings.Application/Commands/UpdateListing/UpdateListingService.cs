using CSharpFunctionalExtensions;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Application.Commands.UpdateListing;

public class UpdateListingService(IListingRepository repository)
{
    public async Task<UnitResult<ErrorList>> Handle(
        UpdateListingCommand command,
        CancellationToken ct = default)
    {
        var listing = await repository.GetByIdAsync(command.ListingId, ct);
        if (listing is null)
            return (ErrorList)Error.NotFound("listing.not_found", "Оголошення не знайдено");

        if (listing.UserId != command.RequestingUserId)
            return (ErrorList)Error.Forbidden("listing.forbidden", "Немає прав для редагування цього оголошення");

        var result = listing.Update(
            command.Title, command.Description, command.SpeciesId, command.BreedId,
            command.AgeMonths, command.Color, command.City,
            command.Vaccinated, command.Castrated, command.Phone);

        if (result.IsFailure)
            return (ErrorList)result.Error;

        await repository.SaveAsync(listing, ct);
        return UnitResult.Success<ErrorList>();
    }
}