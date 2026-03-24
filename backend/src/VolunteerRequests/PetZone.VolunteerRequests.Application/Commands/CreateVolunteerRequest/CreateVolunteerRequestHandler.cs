using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.CreateVolunteerRequest;

public class CreateVolunteerRequestHandler(
    IVolunteerRequestRepository repository,
    IRejectedUserRepository rejectedUserRepository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    ILogger<CreateVolunteerRequestHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateVolunteerRequestCommand command,
        CancellationToken cancellationToken = default)
    {
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

        await repository.AddAsync(requestResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Volunteer request created for user {UserId}", command.UserId);

        return requestResult.Value.Id;
    }
}