using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class UpdatePetStatusService(
    IVolunteerRepository volunteerRepository,
    ILogger<UpdatePetStatusService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdatePetStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating status for pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        var result = pet.UpdateStatus((HelpStatus)command.Request.Status);
        if (result.IsFailure)
            return (ErrorList)result.Error;

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet {PetId} status updated", command.PetId);

        return command.PetId;
    }
}