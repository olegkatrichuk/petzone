using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Application.Commands;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Application.Volunteers;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Infrastructure.Queries;
using PetZone.Volunteers.Presentation.Extensions;
using PetZone.Accounts.Infrastructure.Authorization;

namespace PetZone.Volunteers.Presentation;

public class VolunteersController(
    CreateVolunteerService createVolunteerService,
    UpdateVolunteerMainInfoService updateMainInfoService,
    UpdateVolunteerSocialNetworksService updateSocialNetworksService,
    UpdateVolunteerRequisitesService updateRequisitesService,
    DeleteVolunteerService deleteVolunteerService,
    HardDeleteVolunteerService hardDeleteVolunteerService,
    GetVolunteersHandler getVolunteersHandler,
    GetVolunteerByIdHandler getVolunteerByIdHandler,
    ILogger<VolunteersController> logger) : ControllerBase
{
    
    [Authorize(Policy = Permissions.Volunteers.Create)]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating volunteer. Email: {Email}", request.Email);
        var result = await createVolunteerService.Handle(request.ToCommand(), cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to create volunteer with {Count} errors", result.Error.Errors.Count);
            return result.Error.ToResponse();
        }
        logger.LogInformation("Volunteer created successfully. Id: {VolunteerId}", result.Value);
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Volunteers.Update)]
    [HttpPut("{id:guid}/main-info")]
    public async Task<ActionResult> UpdateMainInfo(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerMainInfoRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating main info for volunteer {VolunteerId}", id);
        var result = await updateMainInfoService.Handle(request.ToCommand(id), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Volunteers.Update)]
    [HttpPut("{id:guid}/social-networks")]
    public async Task<ActionResult> UpdateSocialNetworks(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerSocialNetworksRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating social networks for volunteer {VolunteerId}", id);
        var result = await updateSocialNetworksService.Handle(request.ToCommand(id), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Volunteers.Update)]
    [HttpPut("{id:guid}/requisites")]
    public async Task<ActionResult> UpdateRequisites(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerRequisitesRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating requisites for volunteer {VolunteerId}", id);
        var result = await updateRequisitesService.Handle(request.ToCommand(id), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Volunteers.Delete)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Soft deleting volunteer {VolunteerId}", id);
        var result = await deleteVolunteerService.Handle(id.ToCommand(), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.Volunteers.Delete)]
    [HttpDelete("{id:guid}/hard")]
    public async Task<ActionResult> HardDelete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Hard deleting volunteer {VolunteerId}", id);
        var result = await hardDeleteVolunteerService.Handle(id.ToHardDeleteCommand(), cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }
    
    [Authorize(Policy = Permissions.Volunteers.Read)]
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting volunteers. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetVolunteersQuery(page, pageSize);
        var result = await getVolunteersHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }
    
    [Authorize(Policy = Permissions.Volunteers.Read)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting volunteer by id {VolunteerId}", id);

        var query = new GetVolunteerByIdQuery(id);
        var result = await getVolunteerByIdHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }
}