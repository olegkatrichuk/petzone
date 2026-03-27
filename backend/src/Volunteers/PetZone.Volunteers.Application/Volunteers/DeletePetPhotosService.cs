using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using Microsoft.Extensions.Logging;
using PetZone.Framework.Files;

namespace PetZone.Volunteers.Application.Volunteers;

public class DeletePetPhotosService(
    IVolunteerRepository volunteerRepository,
    IFilesProvider filesProvider,
    ILogger<DeletePetPhotosService> logger)
{
    private const string BucketName = "petzone";

    public async Task<Result<Guid, ErrorList>> Handle(
        DeletePetPhotosCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting photos for pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        foreach (var filePath in command.FilePaths)
        {
            var deleteResult = await filesProvider.DeleteFile(BucketName, filePath, cancellationToken);
            if (deleteResult.IsFailure)
            {
                logger.LogWarning("Failed to delete file {FilePath}: {Error}", filePath, deleteResult.Error.Description);
                return (ErrorList)deleteResult.Error;
            }

            var photoResult = PetPhoto.Create(filePath);
            if (photoResult.IsFailure)
                return (ErrorList)photoResult.Error;

            var removeResult = pet.RemovePhoto(photoResult.Value);
            if (removeResult.IsFailure)
                return (ErrorList)removeResult.Error;
        }

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Deleted {Count} photos for pet {PetId}",
            command.FilePaths.Count(), command.PetId);

        return command.PetId;
    }
}