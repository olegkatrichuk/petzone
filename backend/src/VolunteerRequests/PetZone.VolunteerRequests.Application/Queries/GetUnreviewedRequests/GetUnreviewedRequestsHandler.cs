using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class GetUnreviewedRequestsHandler(
    IVolunteerRequestRepository repository)
{
    public async Task<Result<PagedResult<VolunteerRequest>, Error>> Handle(
        GetUnreviewedRequestsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetUnreviewedAsync(query.Page, query.PageSize, cancellationToken);
        return result;
    }
}