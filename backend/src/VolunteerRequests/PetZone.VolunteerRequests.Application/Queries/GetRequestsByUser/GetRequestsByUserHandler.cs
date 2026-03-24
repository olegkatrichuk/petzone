using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetRequestsByUser;

public class GetRequestsByUserHandler(
    IVolunteerRequestRepository repository)
{
    public async Task<Result<PagedResult<VolunteerRequest>, Error>> Handle(
        GetRequestsByUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetByUserAsync(
            query.UserId,
            query.Status,
            query.Page,
            query.PageSize,
            cancellationToken);

        return result;
    }
}