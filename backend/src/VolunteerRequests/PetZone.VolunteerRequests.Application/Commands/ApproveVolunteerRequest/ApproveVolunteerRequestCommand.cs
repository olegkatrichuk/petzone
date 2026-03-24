namespace PetZone.VolunteerRequests.Application.Commands.ApproveVolunteerRequest;

public record ApproveVolunteerRequestCommand(
    Guid AdminId,
    Guid RequestId
);