using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetRequestsByAdmin;

public record GetRequestsByAdminQuery(
    Guid AdminId,
    VolunteerRequestStatus? Status,
    int Page,
    int PageSize
);