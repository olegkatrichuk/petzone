namespace PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;

public record GetUnreviewedRequestsQuery(
    int Page,
    int PageSize
);