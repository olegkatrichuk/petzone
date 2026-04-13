namespace PetZone.Volunteers.Application.Commands;

public record UpdateApplicationStatusCommand(
    Guid ApplicationId,
    Guid VolunteerId,
    string Action); // "approve" | "reject"