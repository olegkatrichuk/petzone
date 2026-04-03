using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.Listings.Application.Events;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Application.Commands.CreateListing;

public class CreateListingService(
    IListingRepository repository,
    IPublishEndpoint publishEndpoint,
    ILogger<CreateListingService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateListingCommand command,
        CancellationToken ct = default)
    {
        var result = AdoptionListing.Create(
            command.UserId, command.UserName, command.UserEmail, command.UserPhone, command.ContactEmail,
            command.Title, command.Description, command.SpeciesId, command.BreedId,
            command.AgeMonths, command.Color, command.City,
            command.Vaccinated, command.Castrated);

        if (result.IsFailure)
            return result.Error;

        await repository.AddAsync(result.Value, ct);

        try
        {
            await publishEndpoint.Publish(new ListingCreatedEvent(
                result.Value.Id,
                command.UserEmail,
                command.UserName,
                command.Title,
                command.City), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish ListingCreatedEvent for listing {ListingId}", result.Value.Id);
        }

        return result.Value.Id;
    }
}