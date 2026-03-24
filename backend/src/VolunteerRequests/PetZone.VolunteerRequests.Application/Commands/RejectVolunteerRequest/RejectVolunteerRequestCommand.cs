namespace PetZone.VolunteerRequests.Application.Commands.RejectVolunteerRequest;

public record RejectVolunteerRequestCommand(
    Guid AdminId,
    Guid RequestId,
    string Comment
);