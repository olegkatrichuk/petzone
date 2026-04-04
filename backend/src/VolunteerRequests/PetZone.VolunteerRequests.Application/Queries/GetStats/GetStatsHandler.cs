using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Queries.GetStats;

public class GetStatsHandler(IVolunteerRequestRepository repository)
{
    public async Task<Result<VolunteerRequestStats, Error>> Handle(
        GetStatsQuery query,
        CancellationToken cancellationToken = default)
    {
        var stats = await repository.GetStatsAsync(cancellationToken);
        return stats;
    }
}