using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetRequestsByUser;

public record GetRequestsByUserQuery(
    Guid UserId,
    VolunteerRequestStatus? Status,
    int Page,
    int PageSize
);