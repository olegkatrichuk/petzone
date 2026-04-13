namespace PetZone.Volunteers.Contracts;

public record AdoptionApplicationDto(
    Guid Id,
    Guid PetId,
    string PetNickname,
    string? PetMainPhoto,
    Guid VolunteerId,
    Guid ApplicantUserId,
    string ApplicantName,
    string ApplicantPhone,
    string? Message,
    string Status,
    DateTime CreatedAt);

public record CreateAdoptionApplicationRequest(
    string ApplicantName,
    string ApplicantPhone,
    string? Message);

public record UpdateApplicationStatusRequest(string Action);