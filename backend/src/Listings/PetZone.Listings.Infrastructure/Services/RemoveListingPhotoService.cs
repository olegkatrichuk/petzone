using CSharpFunctionalExtensions;
using PetZone.Framework.Files;
using PetZone.Listings.Application.Commands.RemoveListingPhoto;
using PetZone.Listings.Domain;
using PetZone.SharedKernel;

namespace PetZone.Listings.Infrastructure.Services;

public class RemoveListingPhotoService(
    IListingRepository repository,
    IFilesProvider filesProvider)
{
    private const string BucketName = "petzone";

    public async Task<UnitResult<ErrorList>> Handle(
        RemoveListingPhotoCommand command,
        CancellationToken ct = default)
    {
        var listing = await repository.GetByIdAsync(command.ListingId, ct);
        if (listing is null)
            return (ErrorList)Error.NotFound("listing.not_found", "Оголошення не знайдено");

        if (listing.UserId != command.UserId)
            return (ErrorList)Error.Forbidden("listing.forbidden", "Немає доступу до цього оголошення");

        var removeResult = listing.RemovePhoto(command.FileName);
        if (removeResult.IsFailure)
            return (ErrorList)removeResult.Error;

        await repository.SaveAsync(listing, ct);

        await filesProvider.DeleteFile(BucketName, command.FileName, ct);

        return UnitResult.Success<ErrorList>();
    }
}