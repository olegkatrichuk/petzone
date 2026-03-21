namespace PetZone.UseCases.Commands;

public record HardDeletePetCommand(Guid VolunteerId, Guid PetId);