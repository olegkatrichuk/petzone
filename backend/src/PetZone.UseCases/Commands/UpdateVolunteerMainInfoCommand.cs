using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record UpdateVolunteerMainInfoCommand(Guid VolunteerId, UpdateVolunteerMainInfoRequest Request);