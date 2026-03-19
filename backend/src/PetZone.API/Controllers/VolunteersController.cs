using Microsoft.AspNetCore.Mvc;
using PetZone.API.Extensions;
using PetZone.Contracts.Volunteers;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Volunteers;

namespace PetZone.API.Controllers;

[ApiController]
[Route("[controller]")]
public class VolunteersController(
    CreateVolunteerService createVolunteerService,
    UpdateVolunteerMainInfoService updateMainInfoService,
    UpdateVolunteerSocialNetworksService updateSocialNetworksService,
    UpdateVolunteerRequisitesService updateRequisitesService,
    DeleteVolunteerService deleteVolunteerService,
    HardDeleteVolunteerService hardDeleteVolunteerService,
    ILogger<VolunteersController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating volunteer. Email: {Email}", request.Email);
        var command = new CreateVolunteerCommand(request);
        var result = await createVolunteerService.Handle(command, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to create volunteer. Error: {ErrorCode}", result.Error.Code);
            return result.Error.ToResponse();
        }
        logger.LogInformation("Volunteer created successfully. Id: {VolunteerId}", result.Value);
        return this.ToOkResponse(result.Value);
    }

    [HttpPut("{id:guid}/main-info")]
    public async Task<ActionResult> UpdateMainInfo(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerMainInfoRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating main info for volunteer {VolunteerId}", id);
        var command = new UpdateVolunteerMainInfoCommand(id, request);
        var result = await updateMainInfoService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [HttpPut("{id:guid}/social-networks")]
    public async Task<ActionResult> UpdateSocialNetworks(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerSocialNetworksRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating social networks for volunteer {VolunteerId}", id);
        var command = new UpdateVolunteerSocialNetworksCommand(id, request);
        var result = await updateSocialNetworksService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [HttpPut("{id:guid}/requisites")]
    public async Task<ActionResult> UpdateRequisites(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerRequisitesRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating requisites for volunteer {VolunteerId}", id);
        var command = new UpdateVolunteerRequisitesCommand(id, request);
        var result = await updateRequisitesService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Soft deleting volunteer {VolunteerId}", id);
        var command = new DeleteVolunteerCommand(id);
        var result = await deleteVolunteerService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [HttpDelete("{id:guid}/hard")]
    public async Task<ActionResult> HardDelete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Hard deleting volunteer {VolunteerId}", id);
        var command = new HardDeleteVolunteerCommand(id);
        var result = await hardDeleteVolunteerService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }
}