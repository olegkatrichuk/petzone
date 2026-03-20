using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Providers;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class DeletePetPhotosService(
    IVolunteerRepository volunteerRepository,
    IFilesProvider filesProvider,
    ILogger<DeletePetPhotosService> logger)
{
    private const string BucketName = "petzone";

    public async Task<Result<Guid, Error>> Handle(
        DeletePetPhotosCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting photos for pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return Error.NotFound("pet.not_found", "Питомец не найден.");

        // Удаляем файлы из Minio
        foreach (var filePath in command.FilePaths)
        {
            var deleteResult = await filesProvider.DeleteFile(BucketName, filePath, cancellationToken);
            if (deleteResult.IsFailure)
            {
                logger.LogWarning("Failed to delete file {FilePath}: {Error}", filePath, deleteResult.Error.Description);
                return deleteResult.Error;
            }

            // Удаляем из доменной модели
            var photoResult = PetPhoto.Create(filePath);
            if (photoResult.IsFailure)
                return photoResult.Error;

            var removeResult = pet.RemovePhoto(photoResult.Value);
            if (removeResult.IsFailure)
                return removeResult.Error;
        }

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Deleted {Count} photos for pet {PetId}",
            command.FilePaths.Count(), command.PetId);

        return command.PetId;
    }
}