using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Providers;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class HardDeletePetService(
    IVolunteerRepository volunteerRepository,
    IFilesProvider filesProvider,
    ILogger<HardDeletePetService> logger)
{
    private const string BucketName = "petzone";

    public async Task<Result<Guid, ErrorList>> Handle(
        HardDeletePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Hard deleting pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        // Удаляем все фото из Minio
        foreach (var photo in pet.Photos)
        {
            var deleteResult = await filesProvider.DeleteFile(BucketName, photo.FilePath, cancellationToken);
            if (deleteResult.IsFailure)
                logger.LogWarning("Failed to delete photo {FilePath}: {Error}",
                    photo.FilePath, deleteResult.Error.Description);
        }

        var removeResult = volunteer.RemovePet(pet);
        if (removeResult.IsFailure)
            return (ErrorList)removeResult.Error;

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet {PetId} hard deleted", command.PetId);

        return command.PetId;
    }
}