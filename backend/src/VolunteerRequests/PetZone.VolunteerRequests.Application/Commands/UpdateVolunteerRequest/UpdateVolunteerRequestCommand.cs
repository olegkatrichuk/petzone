using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.UpdateVolunteerRequest;

public record UpdateVolunteerRequestCommand(
    Guid UserId,
    Guid RequestId,
    VolunteerInfo VolunteerInfo
);