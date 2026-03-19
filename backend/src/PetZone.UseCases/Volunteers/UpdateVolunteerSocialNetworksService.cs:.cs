using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class UpdateVolunteerSocialNetworksService(
    IVolunteerRepository repository,
    ILogger<UpdateVolunteerSocialNetworksService> logger)
{
    public async Task<Result<Guid, Error>> Handle(
        UpdateVolunteerSocialNetworksCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating social networks for volunteer {VolunteerId}", command.VolunteerId);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        var socialNetworks = new List<SocialNetwork>();
        foreach (var sn in command.Request.SocialNetworks)
        {
            var result = SocialNetwork.Create(sn.Name, sn.Link);
            if (result.IsFailure)
                return result.Error;

            socialNetworks.Add(result.Value);
        }

        volunteer.UpdateSocialNetworks(socialNetworks);
        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} social networks updated", volunteer.Id);

        return volunteer.Id;
    }
}