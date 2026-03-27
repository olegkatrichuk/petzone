using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Events;

namespace PetZone.Volunteers.Application.Volunteers;

public class DeleteVolunteerService(
    IVolunteerRepository repository,
    IPublisher publisher,
    ILogger<DeleteVolunteerService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Soft deleting volunteer {VolunteerId}", command.VolunteerId);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        volunteer.Delete();
        await repository.SoftDeleteAsync(volunteer, cancellationToken);

        await publisher.Publish(
            new VolunteerDeletedEvent(volunteer.Id),
            cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} soft deleted successfully", volunteer.Id);

        return volunteer.Id;
    }
}