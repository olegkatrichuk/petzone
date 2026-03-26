using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Queries.GetDiscussion;

public class GetDiscussionHandler(
    IDiscussionRepository repository)
{
    public async Task<Result<Discussion, Error>> Handle(
        GetDiscussionQuery query,
        CancellationToken cancellationToken = default)
    {
        var discussion = await repository.GetByIdAsync(query.DiscussionId, cancellationToken);
        if (discussion is null)
            return Error.NotFound("discussion.not_found",
                $"Discussion {query.DiscussionId} not found.");

        return discussion;
    }
}