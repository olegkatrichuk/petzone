using CSharpFunctionalExtensions;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Application.Commands.DeleteListing;

public class DeleteListingService(IListingRepository repository, IListingsUnitOfWork unitOfWork)
{
    public async Task<UnitResult<ErrorList>> Handle(
        DeleteListingCommand command,
        CancellationToken ct = default)
    {
        var listing = await repository.GetByIdAsync(command.ListingId, ct);
        if (listing is null)
            return (ErrorList)Error.NotFound("listing.not_found", "Оголошення не знайдено");

        if (listing.UserId != command.RequestingUserId)
            return (ErrorList)Error.Forbidden("listing.forbidden", "Немає прав для видалення цього оголошення");

        repository.Delete(listing);
        await unitOfWork.SaveChangesAsync(ct);
        return UnitResult.Success<ErrorList>();
    }
}