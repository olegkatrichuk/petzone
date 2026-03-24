using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Commands.UpdateVolunteerRequest;

public class UpdateVolunteerRequestHandler(
    IVolunteerRequestRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork,
    ILogger<UpdateVolunteerRequestHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateVolunteerRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return (ErrorList)Error.NotFound("volunteer_request.not_found",
                $"Volunteer request {command.RequestId} not found.");

        if (request.UserId != command.UserId)
            return (ErrorList)Error.Forbidden("volunteer_request.forbidden",
                "Only request owner can update it.");

        var updateResult = request.Update(command.VolunteerInfo);
        if (updateResult.IsFailure)
            return (ErrorList)updateResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Volunteer request {RequestId} updated", command.RequestId);

        return request.Id;
    }
}