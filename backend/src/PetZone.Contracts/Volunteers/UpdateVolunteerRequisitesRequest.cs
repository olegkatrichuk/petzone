
namespace PetZone.Contracts.Volunteers;

public record UpdateVolunteerRequisitesRequest(
    IEnumerable<RequisiteDto> Requisites);