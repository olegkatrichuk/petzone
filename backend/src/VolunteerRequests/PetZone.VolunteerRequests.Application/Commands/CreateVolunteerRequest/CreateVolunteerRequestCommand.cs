using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.CreateVolunteerRequest;

public record CreateVolunteerRequestCommand(
    Guid UserId,
    VolunteerInfo VolunteerInfo
);