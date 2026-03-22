namespace PetZone.Volunteers.Application.Commands;

public record PetPhotoDto(Stream Stream, string FileName);

public record UploadPetPhotosCommand(
    Guid VolunteerId,
    Guid PetId,
    IEnumerable<PetPhotoDto> Photos);