using CSharpFunctionalExtensions;
using PetZone.Listings.Application.Commands.AddListingPhoto;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Infrastructure.Services;

public class AddListingPhotoService(IListingRepository repository)
{
    public async Task<UnitResult<ErrorList>> Handle(
        AddListingPhotoCommand command,
        CancellationToken ct = default)
    {
        var listing = await repository.GetByIdAsync(command.ListingId, ct);
        if (listing is null)
            return (ErrorList)Error.NotFound("listing.not_found", "Оголошення не знайдено");

        if (listing.UserId != command.UserId)
            return (ErrorList)Error.Forbidden("listing.forbidden", "Немає доступу до цього оголошення");

        var result = listing.AddPhoto(command.FileName);
        if (result.IsFailure)
            return (ErrorList)result.Error;

        await repository.SaveAsync(listing, ct);
        return UnitResult.Success<ErrorList>();
    }
}
