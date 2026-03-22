namespace PetZone.Volunteers.Application.Commands;

public record DeletePetCommand(Guid VolunteerId, Guid PetId);