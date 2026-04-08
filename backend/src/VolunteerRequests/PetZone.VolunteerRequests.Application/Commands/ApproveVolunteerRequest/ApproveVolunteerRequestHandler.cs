using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Events;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Commands.ApproveVolunteerRequest;

public class ApproveVolunteerRequestHandler(
    IVolunteerRequestRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    IVolunteerAccountService volunteerAccountService,
    IPublishEndpoint publishEndpoint,
    IUserInfoProvider userInfoProvider,
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

        var userInfo = await userInfoProvider.GetAsync(request.UserId, cancellationToken);
        if (userInfo is null)
            return (ErrorList)Error.NotFound("user.not_found",
                $"User {request.UserId} not found.");

        var accountResult = await volunteerAccountService.CreateAsync(
            userId: request.UserId,
            firstName: userInfo.FirstName,
            lastName: userInfo.LastName,
            email: userInfo.Email,
            phone: userInfo.Phone,
            experienceYears: request.VolunteerInfo.Experience,
            description: request.VolunteerInfo.Motivation,
            cancellationToken: cancellationToken);

        if (accountResult.IsFailure)
            return (ErrorList)accountResult.Error;

        var approveResult = request.Approve();
        if (approveResult.IsFailure)
            return (ErrorList)approveResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new VolunteerRequestStatusChangedEvent(
            UserId: request.UserId,
            RequestId: request.Id,
            Status: "Approved",
            Comment: null,
            Email: userInfo.Email,
            FirstName: userInfo.FirstName,
            LastName: userInfo.LastName), cancellationToken);

        logger.LogInformation("Volunteer request {RequestId} approved", command.RequestId);

        return request.Id;
    }
}
