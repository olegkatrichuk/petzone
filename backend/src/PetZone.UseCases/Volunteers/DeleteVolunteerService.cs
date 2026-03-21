using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class DeleteVolunteerService(
    IVolunteerRepository repository,
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

        logger.LogInformation("Volunteer {VolunteerId} soft deleted successfully", volunteer.Id);

        return volunteer.Id;
    }
}