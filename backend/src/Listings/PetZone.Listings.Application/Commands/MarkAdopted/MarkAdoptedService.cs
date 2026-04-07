using CSharpFunctionalExtensions;
using MassTransit;
using PetZone.Listings.Application.Events;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Application.Commands.MarkAdopted;

public class MarkAdoptedService(
    IListingRepository repository,
    IListingsUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
{
    public async Task<UnitResult<ErrorList>> Handle(
        MarkAdoptedCommand command,
        CancellationToken ct = default)
    {
        var listing = await repository.GetByIdAsync(command.ListingId, ct);
        if (listing is null)
            return (ErrorList)Error.NotFound("listing.not_found", "Оголошення не знайдено");

        if (listing.UserId != command.RequestingUserId)
            return (ErrorList)Error.Forbidden("listing.forbidden", "Немає прав");

        if (listing.Status == ListingStatus.Adopted)
            return (ErrorList)Error.Validation("listing.already_adopted", "Оголошення вже позначено як 'Знайшов дім'");

        listing.MarkAdopted();
        repository.Save(listing);
        await unitOfWork.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new ListingAdoptedEvent(
            listing.Id,
            listing.UserEmail,
            listing.UserName,
            listing.Title), ct);

        return UnitResult.Success<ErrorList>();
    }
}