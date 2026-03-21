using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record SetMainPhotoCommand(Guid VolunteerId, Guid PetId, SetMainPhotoRequest Request);