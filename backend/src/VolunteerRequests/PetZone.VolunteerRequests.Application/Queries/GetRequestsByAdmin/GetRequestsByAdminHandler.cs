using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetRequestsByAdmin;

public class GetRequestsByAdminHandler(
    IVolunteerRequestRepository repository)
{
    public async Task<Result<PagedResult<VolunteerRequest>, Error>> Handle(
        GetRequestsByAdminQuery query,
        CancellationToken cancellationToken = default)
    {
        var status = query.Status ?? VolunteerRequestStatus.OnReview;

        var result = await repository.GetByAdminAsync(
            query.AdminId,
            status,
            query.Page,
            query.PageSize,
            cancellationToken);

        return result;
    }
}