namespace PetZone.Volunteers.Application.Commands;

public record MovePetCommand(Guid VolunteerId, Guid PetId, MovePetRequest Request);