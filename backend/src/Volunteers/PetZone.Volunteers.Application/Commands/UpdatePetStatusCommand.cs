namespace PetZone.Volunteers.Application.Commands;

public record UpdatePetStatusCommand(Guid VolunteerId, Guid PetId, UpdatePetStatusRequest Request);