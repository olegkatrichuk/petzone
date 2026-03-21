using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class DeletePetService(
    IVolunteerRepository volunteerRepository,
    ILogger<DeletePetService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeletePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Soft deleting pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        pet.Delete();
        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet {PetId} soft deleted", command.PetId);

        return command.PetId;
    }
}