using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Events;
using PetZone.Volunteers.Application.Repositories;

namespace PetZone.Volunteers.Application.Commands;

public class UpdateApplicationStatusHandler(
    IAdoptionApplicationRepository applicationRepository,
    IVolunteerRepository volunteerRepository,
    IPublishEndpoint publishEndpoint,
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

        var volunteer = await volunteerRepository.GetByIdAsync(application.VolunteerId, cancellationToken);

        var result = command.Action.ToLower() == "approve"
            ? application.Approve()
            : application.Reject();

        if (result.IsFailure)
            return (ErrorList)result.Error;

        await applicationRepository.SaveAsync(cancellationToken);

        logger.LogInformation("Application {ApplicationId} {Action}d by volunteer {VolunteerId}",
            command.ApplicationId, command.Action, command.VolunteerId);

        if (!string.IsNullOrWhiteSpace(application.ApplicantEmail))
        {
            var volunteerName = volunteer is not null
                ? $"{volunteer.Name.FirstName} {volunteer.Name.LastName}"
                : "Волонтер";

            var petNickname = volunteer?.Pets
                .FirstOrDefault(p => p.Id == application.PetId)?.Nickname ?? "тварина";

            await publishEndpoint.Publish(new AdoptionApplicationStatusChangedEvent(
                ApplicationId: application.Id,
                PetId: application.PetId,
                PetNickname: petNickname,
                ApplicantName: application.ApplicantName,
                ApplicantEmail: application.ApplicantEmail,
                VolunteerName: volunteerName,
                Status: application.Status.ToString()), cancellationToken);
        }

        return UnitResult.Success<ErrorList>();
    }
}