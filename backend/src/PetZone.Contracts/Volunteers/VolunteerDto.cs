namespace PetZone.Contracts.Volunteers;

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
    bool IsDeleted);