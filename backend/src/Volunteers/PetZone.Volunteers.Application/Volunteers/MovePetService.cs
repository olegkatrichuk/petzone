using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using Microsoft.Extensions.Logging;

namespace PetZone.Volunteers.Application.Volunteers;

public class MovePetService(
    IVolunteerRepository repository,
    ILogger<MovePetService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        MovePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Moving pet {PetId} to position {Position}",
            command.PetId, command.Request.NewPosition);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        var result = volunteer.MovePet(pet, command.Request.NewPosition);
        if (result.IsFailure)
            return (ErrorList)result.Error;

        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet {PetId} moved to position {Position}",
            command.PetId, command.Request.NewPosition);

        return command.PetId;
    }
}