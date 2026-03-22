namespace PetZone.Volunteers.Contracts;

public record PetDto(
    Guid Id,
    Guid VolunteerId,
    string Nickname,
    string Color,
    string GeneralDescription,
    string City,
    string Street,
    double Weight,
    double Height,
    bool IsCastrated,
    bool IsVaccinated,
    DateTime DateOfBirth,
    int Status,
    string? MicrochipNumber,
    string? AdoptionConditions,
    Guid SpeciesId,
    Guid BreedId,
    int Position,
    bool IsDeleted,
    IReadOnlyList<PetPhotoDto> Photos);

public record PetPhotoDto(string FilePath, bool IsMain);