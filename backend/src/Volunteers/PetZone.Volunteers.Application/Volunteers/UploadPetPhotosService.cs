using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Application.Providers;

namespace PetZone.Volunteers.Application.Volunteers;

public class UploadPetPhotosService(
    IVolunteerRepository volunteerRepository,
    IFilesProvider filesProvider,
    ILogger<UploadPetPhotosService> logger)
{
    private readonly SemaphoreSlim _semaphore = new(3);
    private const string BucketName = "petzone";

    public async Task<Result<IReadOnlyList<string>, ErrorList>> Handle(
        UploadPetPhotosCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Uploading photos for pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        var uploadTasks = command.Photos.Select(async photo =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return await filesProvider.UploadFile(
                    photo.Stream,
                    BucketName,
                    photo.FileName,
                    cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        var results = await Task.WhenAll(uploadTasks);

        var failedResult = results.FirstOrDefault(r => r.IsFailure);
        if (failedResult.IsFailure)
            return (ErrorList)failedResult.Error;

        var uploadedPaths = results.Select(r => r.Value).ToList();

        foreach (var path in uploadedPaths)
        {
            var photoResult = PetPhoto.Create(path);
            if (photoResult.IsFailure)
                return (ErrorList)photoResult.Error;

            pet.AddPhoto(photoResult.Value);
        }

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Uploaded {Count} photos for pet {PetId}", uploadedPaths.Count, command.PetId);

        return uploadedPaths;
    }
}