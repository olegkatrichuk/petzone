using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record MovePetCommand(Guid VolunteerId, Guid PetId, MovePetRequest Request);