using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Infrastructure.Authorization;
using PetZone.Volunteers.Application.News;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Infrastructure.Queries;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Volunteers.Presentation;

[ApiController]
[Route("news")]
public class NewsController(
    CreateNewsPostService createNewsPostService,
    UpdateNewsPostService updateNewsPostService,
    DeleteNewsPostService deleteNewsPostService,
    GetNewsByVolunteerHandler getNewsByVolunteerHandler,
    ILogger<NewsController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("volunteer/{volunteerId:guid}")]
    public async Task<ActionResult<List<NewsPostDto>>> GetByVolunteer(
        [FromRoute] Guid volunteerId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting news for volunteer {VolunteerId}", volunteerId);
        var result = await getNewsByVolunteerHandler.Handle(volunteerId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Permissions.News.Create)]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateNewsPostRequest request,
        CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var volunteerId))
            return Unauthorized();

        logger.LogInformation("Creating news post for volunteer {VolunteerId}", volunteerId);
        var command = new CreateNewsPostCommand(volunteerId, request.Title, request.Content);
        var result = await createNewsPostService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.News.Update)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateNewsPostRequest request,
        CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var volunteerId))
            return Unauthorized();

        logger.LogInformation("Updating news post {NewsPostId}", id);
        var command = new UpdateNewsPostCommand(id, volunteerId, request.Title, request.Content);
        var result = await updateNewsPostService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [Authorize(Policy = Permissions.News.Delete)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var volunteerId))
            return Unauthorized();

        logger.LogInformation("Deleting news post {NewsPostId}", id);
        var command = new DeleteNewsPostCommand(id, volunteerId);
        var result = await deleteNewsPostService.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }
}