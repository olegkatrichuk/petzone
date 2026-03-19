namespace PetZone.Contracts.Volunteers;

public record UpdateVolunteerMainInfoRequest(
    string FirstName,
    string LastName,
    string Patronymic,
    string Email,
    string GeneralDescription,
    int ExperienceYears,
    string Phone);