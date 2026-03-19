using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record UpdateVolunteerSocialNetworksCommand(Guid VolunteerId, UpdateVolunteerSocialNetworksRequest Request);