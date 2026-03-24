namespace PetZone.VolunteerRequests.Application.Commands.SendForRevision;

public record SendForRevisionCommand(
    Guid AdminId,
    Guid RequestId,
    string Comment
);