using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Framework.Files;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;

namespace PetZone.Volunteers.Application.Volunteers;

public class UploadVolunteerPhotoService(
    IVolunteerRepository volunteerRepository,
    IFilesProvider filesProvider,
    ILogger<UploadVolunteerPhotoService> logger)
{
    private const string BucketName = "petzone";

    public async Task<Result<string, ErrorList>> Handle(
        Guid volunteerId,
        Stream photoStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var volunteer = await volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Volunteer not found");

        var uploadResult = await filesProvider.UploadFile(photoStream, BucketName, fileName, cancellationToken);
        if (uploadResult.IsFailure)
            return (ErrorList)uploadResult.Error;

        volunteer.UpdatePhoto(uploadResult.Value);
        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Photo uploaded for volunteer {VolunteerId}: {Path}", volunteerId, uploadResult.Value);
        return uploadResult.Value;
    }
}