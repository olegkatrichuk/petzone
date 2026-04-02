namespace PetZone.Listings.Application.Commands.CreateListing;

public record CreateListingCommand(
    Guid UserId,
    string UserName,
    string UserEmail,
    string? UserPhone,
    string Title,
    string Description,
    Guid SpeciesId,
    Guid? BreedId,
    int AgeMonths,
    string Color,
    string City,
    bool Vaccinated,
    bool Castrated
);