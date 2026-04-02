namespace PetZone.Volunteers.Contracts;

public record VolunteerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Patronymic,
    string Email,
    string Phone,
    int ExperienceYears,
    string GeneralDescription,
    int PetsCount,
    bool IsDeleted,
    string? PhotoPath,
    IReadOnlyList<SocialNetworkDto> SocialNetworks,
    IReadOnlyList<RequisiteDto> Requisites);