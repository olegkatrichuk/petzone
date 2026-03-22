namespace PetZone.Volunteers.Application.Commands;

public record DeletePetPhotosCommand(
    Guid VolunteerId,
    Guid PetId,
    IEnumerable<string> FilePaths);