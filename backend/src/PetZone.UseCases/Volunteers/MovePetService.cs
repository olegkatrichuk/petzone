using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class MovePetService(
    IVolunteerRepository repository,
    ILogger<MovePetService> logger)
{
    public async Task<Result<Guid, Error>> Handle(
        MovePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Moving pet {PetId} to position {Position}",
            command.PetId, command.Request.NewPosition);

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return Error.NotFound("pet.not_found", "Питомец не найден.");

        var result = volunteer.MovePet(pet, command.Request.NewPosition);
        if (result.IsFailure)
            return result.Error;

        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet {PetId} moved to position {Position}",
            command.PetId, command.Request.NewPosition);

        return command.PetId;
    }
}