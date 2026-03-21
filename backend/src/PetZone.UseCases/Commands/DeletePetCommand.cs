namespace PetZone.UseCases.Commands;

public record DeletePetCommand(Guid VolunteerId, Guid PetId);