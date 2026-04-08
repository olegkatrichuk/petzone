using CSharpFunctionalExtensions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Events;
using PetZone.Core;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.RejectVolunteerRequest;

public class RejectVolunteerRequestHandler(
    IVolunteerRequestRepository repository,
    IRejectedUserRepository rejectedUserRepository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IUserInfoProvider userInfoProvider,
    IValidator<RejectVolunteerRequestCommand> validator,
    ILogger<RejectVolunteerRequestHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        RejectVolunteerRequestCommand command,
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
                "Only assigned admin can reject request.");

        var rejectResult = request.Reject(command.Comment);
        if (rejectResult.IsFailure)
            return (ErrorList)rejectResult.Error;

        var rejectedUser = RejectedUser.Create(request.UserId);
        await rejectedUserRepository.AddAsync(rejectedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userInfo = await userInfoProvider.GetAsync(request.UserId, cancellationToken);

        await publishEndpoint.Publish(new VolunteerRequestStatusChangedEvent(
            UserId: request.UserId,
            RequestId: request.Id,
            Status: "Rejected",
            Comment: command.Comment,
            Email: userInfo?.Email ?? string.Empty,
            FirstName: userInfo?.FirstName ?? string.Empty,
            LastName: userInfo?.LastName ?? string.Empty), cancellationToken);

        logger.LogInformation("Volunteer request {RequestId} rejected", command.RequestId);

        return request.Id;
    }
}