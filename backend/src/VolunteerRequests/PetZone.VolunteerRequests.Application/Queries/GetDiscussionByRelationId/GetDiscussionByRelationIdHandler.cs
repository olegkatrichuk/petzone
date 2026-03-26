using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetDiscussionByRelationId;

public class GetDiscussionByRelationIdHandler(
    IDiscussionRepository repository)
{
    public async Task<Result<Discussion, Error>> Handle(
        GetDiscussionByRelationIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var discussion = await repository.GetByRelationIdAsync(query.RelationId, cancellationToken);
        if (discussion is null)
            return Error.NotFound("discussion.not_found",
                $"Discussion with relation {query.RelationId} not found.");

        return discussion;
    }
}