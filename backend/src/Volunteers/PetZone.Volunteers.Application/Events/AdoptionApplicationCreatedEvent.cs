namespace PetZone.Volunteers.Application.Events;

public record AdoptionApplicationCreatedEvent(
    Guid ApplicationId,
    Guid PetId,
    string PetNickname,
    Guid VolunteerId,
    string VolunteerEmail,
    string VolunteerName,
    string ApplicantName,
    string ApplicantPhone,
    string? Message);