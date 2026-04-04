using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Commands.TakeOnReview;

public record CreateDiscussionCommand(Guid RelationId, List<Guid> Users) : IRequest<Result<Guid, Error>>;

public class TakeOnReviewHandler(
    IVolunteerRequestRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<TakeOnReviewHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        TakeOnReviewCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return (ErrorList)Error.NotFound("volunteer_request.not_found",
                $"Volunteer request {command.RequestId} not found.");

        var discussionResult = await mediator.Send(
            new CreateDiscussionCommand(request.Id, [command.AdminId, request.UserId]),
            cancellationToken);

        if (discussionResult.IsFailure)
            return (ErrorList)discussionResult.Error;

        var takeResult = request.TakeOnReview(command.AdminId, discussionResult.Value);
        if (takeResult.IsFailure)
            return (ErrorList)takeResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Volunteer request {RequestId} taken on review by admin {AdminId}",
            command.RequestId, command.AdminId);

        return request.Id;
    }
}