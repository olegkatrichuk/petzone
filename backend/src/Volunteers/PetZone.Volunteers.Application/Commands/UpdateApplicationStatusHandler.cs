using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;

namespace PetZone.Volunteers.Application.Commands;

public class UpdateApplicationStatusHandler(
    IAdoptionApplicationRepository applicationRepository,
    ILogger<UpdateApplicationStatusHandler> logger)
{
    public async Task<UnitResult<ErrorList>> Handle(
        UpdateApplicationStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var application = await applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);
        if (application is null)
            return (ErrorList)Error.NotFound("adoption.not_found", $"Application {command.ApplicationId} not found.");

        if (application.VolunteerId != command.VolunteerId)
            return (ErrorList)Error.Forbidden("adoption.forbidden", "You do not own this application.");

        if (command.Action.ToLower() is not "approve" and not "reject")
            return (ErrorList)Error.Validation("adoption.invalid_action", "Action must be 'approve' or 'reject'.");

        var result = command.Action.ToLower() == "approve"
            ? application.Approve()
            : application.Reject();

        if (result.IsFailure)
            return (ErrorList)result.Error;

        await applicationRepository.SaveAsync(cancellationToken);

        logger.LogInformation("Application {ApplicationId} {Action}d by volunteer {VolunteerId}",
            command.ApplicationId, command.Action, command.VolunteerId);

        return UnitResult.Success<ErrorList>();
    }
}