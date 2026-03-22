namespace PetZone.Volunteers.Application.Commands;

public record SetMainPhotoCommand(Guid VolunteerId, Guid PetId, SetMainPhotoRequest Request);