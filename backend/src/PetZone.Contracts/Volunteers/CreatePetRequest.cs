namespace PetZone.Contracts.Volunteers;

public record CreatePetRequest(
    string Nickname,
    string GeneralDescription,
    string Color,
    string HealthDescription,
    string? DietOrAllergies,
    string City,
    string Street,
    double Weight,
    double Height,
    string OwnerPhone,
    bool IsCastrated,
    DateTime DateOfBirth,
    bool IsVaccinated,
    int Status,
    string? MicrochipNumber,
    string? AdoptionConditions,
    Guid SpeciesId,
    Guid BreedId);