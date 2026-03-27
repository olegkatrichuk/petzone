using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Events;

namespace PetZone.Volunteers.Application.Volunteers;

public class DeletePetService(
    IVolunteerRepository volunteerRepository,
    IPublisher publisher,
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

        await publisher.Publish(
            new PetDeletedEvent(command.PetId, command.VolunteerId),
            cancellationToken);

        logger.LogInformation("Pet {PetId} soft deleted", command.PetId);

        return command.PetId;
    }
}