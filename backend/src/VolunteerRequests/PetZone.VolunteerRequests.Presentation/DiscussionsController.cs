using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetZone.VolunteerRequests.Application.Commands.AddMessage;
using PetZone.VolunteerRequests.Application.Commands.CloseDiscussion;
using PetZone.VolunteerRequests.Application.Commands.DeleteMessage;
using PetZone.VolunteerRequests.Application.Commands.EditMessage;
using PetZone.VolunteerRequests.Application.Queries.GetDiscussion;
using PetZone.Volunteers.Presentation.Extensions;
using System.Security.Claims;
using PetZone.VolunteerRequests.Application.Queries.GetDiscussionByRelationId;

namespace PetZone.VolunteerRequests.Presentation;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DiscussionsController : ControllerBase
{
    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    // POST /discussions/{discussionId}/messages
    [HttpPost("{discussionId:guid}/messages")]
    public async Task<IActionResult> AddMessage(
        [FromRoute] Guid discussionId,
        [FromBody] AddMessageDto dto,
        [FromServices] AddMessageHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new AddMessageCommand(userId.Value, discussionId, dto.Text);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    // DELETE /discussions/{discussionId}/messages/{messageId}
    [HttpDelete("{discussionId:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(
        [FromRoute] Guid discussionId,
        [FromRoute] Guid messageId,
        [FromServices] DeleteMessageHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new DeleteMessageCommand(userId.Value, discussionId, messageId);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    // PUT /discussions/{discussionId}/messages/{messageId}
    [HttpPut("{discussionId:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> EditMessage(
        [FromRoute] Guid discussionId,
        [FromRoute] Guid messageId,
        [FromBody] EditMessageDto dto,
        [FromServices] EditMessageHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new EditMessageCommand(userId.Value, discussionId, messageId, dto.NewText);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    // PUT /discussions/{discussionId}/close
    [HttpPut("{discussionId:guid}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Close(
        [FromRoute] Guid discussionId,
        [FromServices] CloseDiscussionHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CloseDiscussionCommand(discussionId);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    // GET /discussions/{discussionId}
    [HttpGet("{discussionId:guid}")]
    public async Task<IActionResult> GetDiscussion(
        [FromRoute] Guid discussionId,
        [FromServices] GetDiscussionHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetDiscussionQuery(discussionId);
        var result = await handler.Handle(query, cancellationToken);
        if (result.IsFailure) return result.Error.ToResponse();

        var isParticipant = result.Value.Users.Contains(userId.Value);
        if (!isParticipant && !User.IsInRole("Admin")) return Forbid();

        return this.ToOkResponse(result.Value);
    }

    // GET /discussions/by-relation/{relationId}
    [HttpGet("by-relation/{relationId:guid}")]
    public async Task<IActionResult> GetByRelationId(
        [FromRoute] Guid relationId,
        [FromServices] GetDiscussionByRelationIdHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetDiscussionByRelationIdQuery(relationId);
        var result = await handler.Handle(query, cancellationToken);
        if (result.IsFailure) return result.Error.ToResponse();

        var isParticipant = result.Value.Users.Contains(userId.Value);
        if (!isParticipant && !User.IsInRole("Admin")) return Forbid();

        return this.ToOkResponse(result.Value);
    }
}

public record AddMessageDto(string Text);
public record EditMessageDto(string NewText);