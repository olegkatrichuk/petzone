using CSharpFunctionalExtensions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Events;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.CreateVolunteerRequest;

public class CreateVolunteerRequestHandler(
    IVolunteerRequestRepository repository,
    IRejectedUserRepository rejectedUserRepository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    IVolunteerAccountService volunteerAccountService,
    IUserInfoProvider userInfoProvider,
    IPublishEndpoint publishEndpoint,
    IValidator<CreateVolunteerRequestCommand> validator,
    ILogger<CreateVolunteerRequestHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateVolunteerRequestCommand command,
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

        var rejectedUser = await rejectedUserRepository
            .GetByUserIdAsync(command.UserId, cancellationToken);

        if (rejectedUser is not null && rejectedUser.IsBlocked())
            return (ErrorList)Error.Conflict("volunteer_request.user_blocked",
                $"User is blocked until {rejectedUser.RejectedUntil:yyyy-MM-dd}.");

        var activeRequest = await repository
            .GetActiveByUserIdAsync(command.UserId, cancellationToken);

        if (activeRequest is not null)
            return (ErrorList)Error.Conflict("volunteer_request.already_exists",
                "User already has an active volunteer request.");

        var requestResult = VolunteerRequest.Create(command.UserId, command.VolunteerInfo);
        if (requestResult.IsFailure)
            return (ErrorList)requestResult.Error;

        var request = requestResult.Value;

        var userInfo = await userInfoProvider.GetAsync(command.UserId, cancellationToken);
        if (userInfo is null)
            return (ErrorList)Error.NotFound("user.not_found", $"User {command.UserId} not found.");

        var accountResult = await volunteerAccountService.CreateAsync(
            userId: command.UserId,
            firstName: userInfo.FirstName,
            lastName: userInfo.LastName,
            email: userInfo.Email,
            phone: userInfo.Phone,
            experienceYears: command.VolunteerInfo.Experience,
            description: command.VolunteerInfo.Motivation,
            cancellationToken: cancellationToken);

        if (accountResult.IsFailure)
            return (ErrorList)accountResult.Error;

        var autoApproveResult = request.AutoApprove();
        if (autoApproveResult.IsFailure)
            return (ErrorList)autoApproveResult.Error;

        await repository.AddAsync(request, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new VolunteerRequestStatusChangedEvent(
            UserId: request.UserId,
            RequestId: request.Id,
            Status: "Approved",
            Comment: null,
            Email: userInfo.Email,
            FirstName: userInfo.FirstName,
            LastName: userInfo.LastName), cancellationToken);

        logger.LogInformation("Volunteer request auto-approved for user {UserId}", command.UserId);

        return request.Id;
    }
}