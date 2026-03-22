using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using Microsoft.Extensions.Logging;

namespace PetZone.Volunteers.Application.Volunteers;

public class UpdateVolunteerSocialNetworksService(
    IVolunteerRepository repository,
    ILogger<UpdateVolunteerSocialNetworksService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateVolunteerSocialNetworksCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating social networks for volunteer {VolunteerId}", command.VolunteerId);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        var socialNetworks = new List<SocialNetwork>();
        foreach (var sn in command.Request.SocialNetworks)
        {
            var result = SocialNetwork.Create(sn.Name, sn.Link);
            if (result.IsFailure)
                return (ErrorList)result.Error;

            socialNetworks.Add(result.Value);
        }

        volunteer.UpdateSocialNetworks(socialNetworks);
        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} social networks updated", volunteer.Id);

        return volunteer.Id;
    }
}