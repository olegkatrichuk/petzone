namespace PetZone.VolunteerRequests.Application.Events;

public record VolunteerRequestStatusChangedEvent(
    Guid UserId,
    Guid RequestId,
    string Status,
    string? Comment,
    string Email,
    string FirstName,
    string LastName
);
