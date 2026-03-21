using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record UpdatePetCommand(Guid VolunteerId, Guid PetId, UpdatePetRequest Request);