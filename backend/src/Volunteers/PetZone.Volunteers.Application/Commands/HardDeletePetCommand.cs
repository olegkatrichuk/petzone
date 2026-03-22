namespace PetZone.Volunteers.Application.Commands;

public record HardDeletePetCommand(Guid VolunteerId, Guid PetId);