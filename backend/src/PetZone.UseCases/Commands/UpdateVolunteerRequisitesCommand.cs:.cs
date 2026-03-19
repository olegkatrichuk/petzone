using PetZone.Contracts.Volunteers;

namespace PetZone.UseCases.Commands;

public record UpdateVolunteerRequisitesCommand(Guid VolunteerId, UpdateVolunteerRequisitesRequest Request);