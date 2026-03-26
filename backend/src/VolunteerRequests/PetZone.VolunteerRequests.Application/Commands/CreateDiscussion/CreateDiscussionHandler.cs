using CSharpFunctionalExtensions;
using MediatR;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.TakeOnReview;

public class CreateDiscussionHandler(
    IDiscussionRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork)
    : IRequestHandler<CreateDiscussionCommand, Result<Guid, Error>>
{
    public async Task<Result<Guid, Error>> Handle(
        CreateDiscussionCommand request,
        CancellationToken cancellationToken = default)
    {
        var discussionResult = Discussion.Create(request.RelationId, request.Users);
        if (discussionResult.IsFailure)
            return discussionResult.Error;

        await repository.AddAsync(discussionResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return discussionResult.Value.Id;
    }
}