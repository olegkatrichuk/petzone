
namespace PetZone.Volunteers.Contracts;

public record UpdateVolunteerRequisitesRequest(
    IEnumerable<RequisiteDto> Requisites);