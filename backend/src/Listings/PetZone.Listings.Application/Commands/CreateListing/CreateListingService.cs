using CSharpFunctionalExtensions;
using FluentValidation;
using MassTransit;
using PetZone.Listings.Application.Events;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Application.Commands.CreateListing;

public class CreateListingService(
    IListingRepository repository,
    IListingsUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IValidator<CreateListingCommand> validator)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateListingCommand command,
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

        var alreadyExists = await repository.ActiveListingExistsAsync(command.UserId, command.Title, ct);
        if (alreadyExists)
            return (ErrorList)Error.Conflict("listing.duplicate",
                "У вас вже є активне оголошення з такою назвою.");

        var result = AdoptionListing.Create(
            command.UserId, command.UserName, command.UserEmail, command.UserPhone, command.ContactEmail,
            command.Title, command.Description, command.SpeciesId, command.BreedId,
            command.AgeMonths, command.Color, command.City,
            command.Vaccinated, command.Castrated);

        if (result.IsFailure)
            return result.Error;

        await repository.AddAsync(result.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await publishEndpoint.Publish(new ListingCreatedEvent(
            result.Value.Id,
            command.UserEmail,
            command.UserName,
            command.Title,
            command.City), ct);

        return result.Value.Id;
    }
}