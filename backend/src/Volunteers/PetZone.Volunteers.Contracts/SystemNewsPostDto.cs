namespace PetZone.Volunteers.Contracts;

public record SystemNewsPostDto(
    Guid Id,
    string Type,
    DateTime PublishedAt,
    int LookingForHome,
    int NeedsHelp,
    int FoundHomeThisWeek,
    int TotalVolunteers,
    string FactEn,
    string TopBreedsJson,
    string? TopCity,
    string? FeaturedPetNickname,
    string? FeaturedPetPhotoUrl,
    string? FeaturedPetDescription,
    string? FeaturedPetBreed,
    string? FeaturedPetCity);
