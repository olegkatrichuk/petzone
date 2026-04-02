using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Infrastructure.Authorization;
using PetZone.Species.Application.Commands;
using PetZone.Species.Application.Queries;
using PetZone.Species.Contracts;
using PetZone.Species.Infrastructure.Queries;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Species.Presentation;

[ApiController]
[Route("[controller]")]
public class SpeciesController(
    GetAllSpeciesHandler getAllSpeciesHandler,
    GetBreedsBySpeciesIdHandler getBreedsBySpeciesIdHandler,
    DeleteSpeciesService deleteSpeciesService,
    DeleteBreedService deleteBreedService,
    CreateSpeciesService createSpeciesService,
    CreateBreedService createBreedService,
    ILogger<SpeciesController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] string locale = "uk",
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all species");
        var result = await getAllSpeciesHandler.Handle(new GetAllSpeciesQuery(locale), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [AllowAnonymous]
    [HttpGet("{speciesId:guid}/breeds")]
    public async Task<ActionResult> GetBreeds(
        [FromRoute] Guid speciesId,
        [FromQuery] string locale = "uk",
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting breeds for species {SpeciesId}", speciesId);
        var result = await getBreedsBySpeciesIdHandler.Handle(
            new GetBreedsBySpeciesIdQuery(speciesId, locale), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Species.Create)]
    [HttpPost]
    public async Task<ActionResult> CreateSpecies(
        [FromBody] CreateSpeciesRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating species");
        var result = await createSpeciesService.Handle(
            new CreateSpeciesCommand(request.Translations), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Species.Create)]
    [HttpPost("{speciesId:guid}/breeds")]
    public async Task<ActionResult> CreateBreed(
        [FromRoute] Guid speciesId,
        [FromBody] CreateBreedRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating breed for species {SpeciesId}", speciesId);
        var result = await createBreedService.Handle(
            new CreateBreedCommand(speciesId, request.Translations), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Species.Delete)]
    [HttpDelete("{speciesId:guid}")]
    public async Task<ActionResult> DeleteSpecies(
        [FromRoute] Guid speciesId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting species {SpeciesId}", speciesId);
        var result = await deleteSpeciesService.Handle(
            new DeleteSpeciesCommand(speciesId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Species.Delete)]
    [HttpDelete("{speciesId:guid}/breeds/{breedId:guid}")]
    public async Task<ActionResult> DeleteBreed(
        [FromRoute] Guid speciesId,
        [FromRoute] Guid breedId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting breed {BreedId} from species {SpeciesId}", breedId, speciesId);
        var result = await deleteBreedService.Handle(
            new DeleteBreedCommand(speciesId, breedId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }
}