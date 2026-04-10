using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Infrastructure.Authorization;
using PetZone.Volunteers.Application.Commands;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Application.Volunteers;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Presentation.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using PetPhotoDto = PetZone.Volunteers.Application.Commands.PetPhotoDto;

namespace PetZone.Volunteers.Presentation;

[Authorize]
[ApiController]
[Route("volunteers/{volunteerId:guid}/pets")]
public class PetsController(
    CreatePetService createPetService,
    UpdatePetService updatePetService,
    UpdatePetStatusService updatePetStatusService,
    DeletePetService deletePetService,
    HardDeletePetService hardDeletePetService,
    SetMainPhotoService setMainPhotoService,
    UploadPetPhotosService uploadPetPhotosService,
    DeletePetPhotosService deletePetPhotosService,
    MovePetService movePetService,
    IVolunteerRepository volunteerRepository,
    ILogger<PetsController> logger) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    /// <summary>Returns true if the requesting user owns the volunteer profile or is Admin.</summary>
    private async Task<bool> IsOwnerOrAdminAsync(Guid volunteerId, CancellationToken ct)
    {
        if (User.IsInRole("Admin")) return true;
        var userId = GetUserId();
        if (userId is null) return false;
        var volunteer = await volunteerRepository.GetByIdAsync(volunteerId, ct);
        return volunteer is not null && volunteer.UserId == userId.Value;
    }

    [Authorize(Policy = Permissions.Pets.Create)]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromRoute] Guid volunteerId,
        [FromBody] CreatePetRequest request,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Creating pet for volunteer {VolunteerId}", volunteerId);
        var result = await createPetService.Handle(request.ToCommand(volunteerId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.UploadPhotos)]
    [HttpPost("{petId:guid}/photos")]
    public async Task<ActionResult> UploadPhotos(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromForm] IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Uploading {Count} photos for pet {PetId}", files.Count, petId);

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return BadRequest(
                    $"Недопустимый тип файла: {extension}. Разрешены: {string.Join(", ", AllowedExtensions)}");

            if (file.Length > MaxFileSize)
                return BadRequest($"Файл {file.FileName} превышает максимальный размер 5MB.");
        }

        var photos = new List<PetPhotoDto>();
        foreach (var file in files)
        {
            var webpStream = await ConvertToWebpAsync(file);
            var fileName = $"{Guid.NewGuid()}.webp";
            photos.Add(new PetPhotoDto(webpStream, fileName));
        }

        var command = new UploadPetPhotosCommand(volunteerId, petId, photos);
        var result = await uploadPetPhotosService.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.DeletePhotos)]
    [HttpDelete("{petId:guid}/photos")]
    public async Task<ActionResult> DeletePhotos(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] IEnumerable<string> filePaths,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Deleting photos for pet {PetId}", petId);
        var command = new DeletePetPhotosCommand(volunteerId, petId, filePaths);
        var result = await deletePetPhotosService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.Move)]
    [HttpPut("{petId:guid}/position")]
    public async Task<ActionResult> MovePet(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] MovePetRequest request,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Moving pet {PetId} to position {Position}", petId, request.NewPosition);
        var result = await movePetService.Handle(request.ToCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    private const int MaxImageDimension = 4000;

    private static async Task<Stream> ConvertToWebpAsync(IFormFile file)
    {
        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream);

        // Downscale to stay within limits while preserving aspect ratio
        if (image.Width > MaxImageDimension || image.Height > MaxImageDimension)
        {
            var ratio = Math.Min(
                (double)MaxImageDimension / image.Width,
                (double)MaxImageDimension / image.Height);
            image.Mutate(ctx => ctx.Resize(
                (int)(image.Width * ratio),
                (int)(image.Height * ratio)));
        }

        var outputStream = new MemoryStream();
        var encoder = new WebpEncoder { Quality = 80 };
        await image.SaveAsync(outputStream, encoder);
        outputStream.Position = 0;

        return outputStream;
    }

    [Authorize(Policy = Permissions.Pets.Update)]
    [HttpPut("{petId:guid}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] UpdatePetRequest request,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Updating pet {PetId}", petId);
        var result = await updatePetService.Handle(request.ToCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.UpdateStatus)]
    [HttpPut("{petId:guid}/status")]
    public async Task<ActionResult> UpdateStatus(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] UpdatePetStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Updating status for pet {PetId}", petId);
        var result = await updatePetStatusService.Handle(request.ToCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.Delete)]
    [HttpDelete("{petId:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Soft deleting pet {PetId}", petId);
        var result = await deletePetService.Handle(new DeletePetCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.Delete)]
    [HttpDelete("{petId:guid}/hard")]
    public async Task<ActionResult> HardDelete(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Hard deleting pet {PetId}", petId);
        var result = await hardDeletePetService.Handle(new HardDeletePetCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Pets.SetMainPhoto)]
    [HttpPut("{petId:guid}/main-photo")]
    public async Task<ActionResult> SetMainPhoto(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] SetMainPhotoRequest request,
        CancellationToken cancellationToken)
    {
        if (!await IsOwnerOrAdminAsync(volunteerId, cancellationToken)) return Forbid();
        logger.LogInformation("Setting main photo for pet {PetId}", petId);
        var result = await setMainPhotoService.Handle(request.ToCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }
}