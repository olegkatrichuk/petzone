using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class HardDeleteVolunteerService(
    IVolunteerRepository repository,
    ILogger<HardDeleteVolunteerService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        HardDeleteVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Hard deleting volunteer {VolunteerId}", command.VolunteerId);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        await repository.HardDeleteAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} hard deleted successfully", command.VolunteerId);

        return command.VolunteerId;
    }
}