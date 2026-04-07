using CSharpFunctionalExtensions;
using FluentValidation;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Application.Commands.UpdateListing;

public class UpdateListingService(
    IListingRepository repository,
    IListingsUnitOfWork unitOfWork,
    IValidator<UpdateListingCommand> validator)
{
    public async Task<UnitResult<ErrorList>> Handle(
        UpdateListingCommand command,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage))
                .ToList();
            return new ErrorList(errors);
        }

        var listing = await repository.GetByIdAsync(command.ListingId, ct);
        if (listing is null)
            return (ErrorList)Error.NotFound("listing.not_found", "Оголошення не знайдено");

        if (listing.UserId != command.RequestingUserId)
            return (ErrorList)Error.Forbidden("listing.forbidden", "Немає прав для редагування цього оголошення");

        if (listing.Status == ListingStatus.Removed)
            return (ErrorList)Error.Validation("listing.cannot_edit_removed", "Неможливо редагувати видалене оголошення");

        var result = listing.Update(
            command.Title, command.Description, command.SpeciesId, command.BreedId,
            command.AgeMonths, command.Color, command.City,
            command.Vaccinated, command.Castrated, command.Phone, command.ContactEmail);

        if (result.IsFailure)
            return (ErrorList)result.Error;

        repository.Save(listing);
        await unitOfWork.SaveChangesAsync(ct);
        return UnitResult.Success<ErrorList>();
    }
}