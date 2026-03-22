using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using Microsoft.Extensions.Logging;

namespace PetZone.Volunteers.Application.Volunteers;

public class UpdateVolunteerRequisitesService(
    IVolunteerRepository repository,
    ILogger<UpdateVolunteerRequisitesService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateVolunteerRequisitesCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating requisites for volunteer {VolunteerId}", command.VolunteerId);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        var requisites = new List<Requisite>();
        foreach (var r in command.Request.Requisites)
        {
            var result = Requisite.Create(r.Name, r.Description);
            if (result.IsFailure)
                return (ErrorList)result.Error;

            requisites.Add(result.Value);
        }

        volunteer.UpdateRequisites(requisites);
        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} requisites updated", volunteer.Id);

        return volunteer.Id;
    }
}