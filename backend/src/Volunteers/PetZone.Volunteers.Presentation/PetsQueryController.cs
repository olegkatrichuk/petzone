using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Infrastructure.Queries;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Volunteers.Presentation;

[ApiController]
[Route("pets")]
public class PetsQueryController(
    GetPetsHandler getPetsHandler,
    GetPetByIdHandler getPetByIdHandler,
    ILogger<PetsQueryController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? volunteerId = null,
        [FromQuery] string? nickname = null,
        [FromQuery] string? color = null,
        [FromQuery] string? city = null,
        [FromQuery] Guid? speciesId = null,
        [FromQuery] Guid? breedId = null,
        [FromQuery] int? minAge = null,
        [FromQuery] int? maxAge = null,
        [FromQuery] double? minWeight = null,
        [FromQuery] double? maxWeight = null,
        [FromQuery] bool? isCastrated = null,
        [FromQuery] bool? isVaccinated = null,
        [FromQuery] int? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting pets with filters. Page: {Page}", page);

        var query = new GetPetsQuery(
            page, pageSize, volunteerId, nickname, color, city,
            speciesId, breedId, minAge, maxAge, minWeight, maxWeight,
            isCastrated, isVaccinated, status, sortBy, sortDescending);

        var result = await getPetsHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [HttpGet("{petId:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid petId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting pet by id {PetId}", petId);

        var result = await getPetByIdHandler.Handle(
            new GetPetByIdQuery(petId), cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }
}