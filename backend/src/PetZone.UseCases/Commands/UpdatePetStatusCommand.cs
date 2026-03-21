using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record UpdatePetStatusCommand(Guid VolunteerId, Guid PetId, UpdatePetStatusRequest Request);