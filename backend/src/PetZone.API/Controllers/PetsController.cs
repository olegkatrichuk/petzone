using Microsoft.AspNetCore.Mvc;
using PetZone.API.Extensions;
using PetZone.API.Extensions.Requests;
using PetZone.Contracts.Volunteers;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Volunteers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace PetZone.API.Controllers;

[ApiController]
[Route("volunteers/{volunteerId:guid}/pets")]
public class PetsController(
    CreatePetService createPetService,
    UploadPetPhotosService uploadPetPhotosService,
    DeletePetPhotosService deletePetPhotosService,
    MovePetService movePetService,
    ILogger<PetsController> logger) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromRoute] Guid volunteerId,
        [FromBody] CreatePetRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating pet for volunteer {VolunteerId}", volunteerId);
        var result = await createPetService.Handle(request.ToCommand(volunteerId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [HttpPost("{petId:guid}/photos")]
    public async Task<ActionResult> UploadPhotos(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromForm] IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Uploading {Count} photos for pet {PetId}", files.Count, petId);

        // Валидация файлов
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return BadRequest($"Недопустимый тип файла: {extension}. Разрешены: {string.Join(", ", AllowedExtensions)}");

            if (file.Length > MaxFileSize)
                return BadRequest($"Файл {file.FileName} превышает максимальный размер 5MB.");
        }

        // Конвертируем в WebP и загружаем
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

    [HttpDelete("{petId:guid}/photos")]
    public async Task<ActionResult> DeletePhotos(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] IEnumerable<string> filePaths,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting photos for pet {PetId}", petId);
        var command = new DeletePetPhotosCommand(volunteerId, petId, filePaths);
        var result = await deletePetPhotosService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [HttpPut("{petId:guid}/position")]
    public async Task<ActionResult> MovePet(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] MovePetRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Moving pet {PetId} to position {Position}", petId, request.NewPosition);
        var result = await movePetService.Handle(request.ToCommand(volunteerId, petId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    // Конвертация в WebP с качеством 80%
    private static async Task<Stream> ConvertToWebpAsync(IFormFile file)
    {
        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream);

        var outputStream = new MemoryStream();
        var encoder = new WebpEncoder { Quality = 80 };
        await image.SaveAsync(outputStream, encoder);
        outputStream.Position = 0;

        return outputStream;
    }
}