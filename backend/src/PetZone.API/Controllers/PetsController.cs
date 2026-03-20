using Microsoft.AspNetCore.Mvc;
using PetZone.API.Extensions;
using PetZone.API.Extensions.Requests;
using PetZone.Contracts.Volunteers;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Volunteers;

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

        var photos = files.Select(f => new PetPhotoDto(
            f.OpenReadStream(),
            Guid.NewGuid() + Path.GetExtension(f.FileName)));

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
}