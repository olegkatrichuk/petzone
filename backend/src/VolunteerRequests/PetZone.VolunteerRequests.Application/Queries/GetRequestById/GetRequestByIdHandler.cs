using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetRequestById;

public class GetRequestByIdHandler(IVolunteerRequestRepository repository)
{
    public async Task<Result<VolunteerRequest, Error>> Handle(
        GetRequestByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(query.RequestId, cancellationToken);

        if (request is null)
            return Error.NotFound("volunteer_request.not_found", "Заявка не найдена.");

        return request;
    }
}