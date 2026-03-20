namespace PetZone.UseCases.Commands;

public record DeletePetPhotosCommand(
    Guid VolunteerId,
    Guid PetId,
    IEnumerable<string> FilePaths);