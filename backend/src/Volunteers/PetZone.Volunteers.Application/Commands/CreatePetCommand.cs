namespace PetZone.Volunteers.Application.Commands;

public record CreatePetCommand(Guid VolunteerId, CreatePetRequest Request);