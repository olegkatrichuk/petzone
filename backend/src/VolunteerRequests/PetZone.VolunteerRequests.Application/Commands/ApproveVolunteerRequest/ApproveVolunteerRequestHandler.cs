using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Commands.ApproveVolunteerRequest;

public record CreateVolunteerAccountCommand(Guid UserId, Domain.VolunteerInfo VolunteerInfo)
    : IRequest<Result<Guid, Error>>;

public class ApproveVolunteerRequestHandler(
    IVolunteerRequestRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<ApproveVolunteerRequestHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        ApproveVolunteerRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return (ErrorList)Error.NotFound("volunteer_request.not_found",
                $"Volunteer request {command.RequestId} not found.");

        if (request.AdminId != command.AdminId)
            return (ErrorList)Error.Forbidden("volunteer_request.forbidden",
                "Only assigned admin can approve request.");

        var accountResult = await mediator.Send(
            new CreateVolunteerAccountCommand(request.UserId, request.VolunteerInfo),
            cancellationToken);

        if (accountResult.IsFailure)
            return (ErrorList)accountResult.Error;

        var approveResult = request.Approve(Guid.NewGuid());
        if (approveResult.IsFailure)
            return (ErrorList)approveResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Volunteer request {RequestId} approved", command.RequestId);

        return request.Id;
    }
}