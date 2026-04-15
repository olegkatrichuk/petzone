namespace PetZone.Volunteers.Application.Events;

public record AdoptionApplicationStatusChangedEvent(
    Guid ApplicationId,
    Guid PetId,
    string PetNickname,
    string ApplicantName,
    string ApplicantEmail,
    string VolunteerName,
    string Status); // "Approved" | "Rejected"