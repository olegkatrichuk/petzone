namespace PetZone.Volunteers.Application.Commands;

public record CreateAdoptionApplicationCommand(
    Guid PetId,
    Guid VolunteerId,
    Guid ApplicantUserId,
    string ApplicantName,
    string ApplicantEmail,
    string ApplicantPhone,
    string? Message);