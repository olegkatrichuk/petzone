namespace PetZone.Listings.Application.Commands.UpdateListing;

public record UpdateListingCommand(
    Guid ListingId,
    Guid RequestingUserId,
    string Title,
    string Description,
    Guid SpeciesId,
    Guid? BreedId,
    int AgeMonths,
    string Color,
    string City,
    bool Vaccinated,
    bool Castrated,
    string? Phone
);