namespace PetZone.Volunteers.Application.Commands;

public record UpdatePetCommand(Guid VolunteerId, Guid PetId, UpdatePetRequest Request);