using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record CreatePetCommand(Guid VolunteerId, CreatePetRequest Request);