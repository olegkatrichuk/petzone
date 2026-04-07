using CSharpFunctionalExtensions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Events;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Commands.SendForRevision;

public class SendForRevisionHandler(
    IVolunteerRequestRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IValidator<SendForRevisionCommand> validator,
    ILogger<SendForRevisionHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        SendForRevisionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage))
                .ToList();
            return new ErrorList(errors);
        }

        var request = await repository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return (ErrorList)Error.NotFound("volunteer_request.not_found",
                $"Volunteer request {command.RequestId} not found.");

        if (request.AdminId != command.AdminId)
            return (ErrorList)Error.Forbidden("volunteer_request.forbidden",
                "Only assigned admin can send request for revision.");

        var result = request.SendForRevision(command.Comment);
        if (result.IsFailure)
            return (ErrorList)result.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new VolunteerRequestStatusChangedEvent(
            UserId: request.UserId,
            RequestId: request.Id,
            Status: "RevisionRequired",
            Comment: command.Comment), cancellationToken);

        logger.LogInformation("Volunteer request {RequestId} sent for revision", command.RequestId);

        return request.Id;
    }
}