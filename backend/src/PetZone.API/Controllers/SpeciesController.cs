using Microsoft.AspNetCore.Mvc;
using PetZone.API.Extensions;
using PetZone.Infrastructure.Queries;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Queries;

namespace PetZone.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeciesController(
    GetAllSpeciesHandler getAllSpeciesHandler,
    GetBreedsBySpeciesIdHandler getBreedsBySpeciesIdHandler,
    DeleteSpeciesService deleteSpeciesService,
    DeleteBreedService deleteBreedService,
    ILogger<SpeciesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all species");
        var result = await getAllSpeciesHandler.Handle(new GetAllSpeciesQuery(), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [HttpGet("{speciesId:guid}/breeds")]
    public async Task<ActionResult> GetBreeds(
        [FromRoute] Guid speciesId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting breeds for species {SpeciesId}", speciesId);
        var result = await getBreedsBySpeciesIdHandler.Handle(
            new GetBreedsBySpeciesIdQuery(speciesId), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

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