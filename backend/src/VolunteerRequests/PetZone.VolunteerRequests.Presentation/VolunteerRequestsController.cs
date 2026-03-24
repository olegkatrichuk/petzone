using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetZone.VolunteerRequests.Application.Commands.ApproveVolunteerRequest;
using PetZone.VolunteerRequests.Application.Commands.CreateVolunteerRequest;
using PetZone.VolunteerRequests.Application.Commands.RejectVolunteerRequest;
using PetZone.VolunteerRequests.Application.Commands.SendForRevision;
using PetZone.VolunteerRequests.Application.Commands.TakeOnReview;
using PetZone.VolunteerRequests.Application.Commands.UpdateVolunteerRequest;
using PetZone.VolunteerRequests.Application.Queries.GetRequestsByAdmin;
using PetZone.VolunteerRequests.Application.Queries.GetRequestsByUser;
using PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;
using PetZone.VolunteerRequests.Domain;
using PetZone.VolunteerRequests.Presentation.Requests;
using PetZone.Volunteers.Presentation.Extensions;
using System.Security.Claims;

namespace PetZone.VolunteerRequests.Presentation;

[ApiController]
[Route("[controller]")]
public class VolunteerRequestsController : ControllerBase
{
    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    // POST /volunteerrequests
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateRequest(
        [FromBody] CreateVolunteerRequestDto dto,
        [FromServices] CreateVolunteerRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CreateVolunteerRequestCommand(
            userId.Value,
            new VolunteerInfo(dto.Experience, dto.Certificates, dto.Requisites));

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpPut("{requestId:guid}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TakeOnReview(
        [FromRoute] Guid requestId,
        [FromServices] TakeOnReviewHandler handler,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized();

        var command = new TakeOnReviewCommand(adminId.Value, requestId);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpPut("{requestId:guid}/revision")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendForRevision(
        [FromRoute] Guid requestId,
        [FromBody] SendForRevisionDto dto,
        [FromServices] SendForRevisionHandler handler,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized();

        var command = new SendForRevisionCommand(adminId.Value, requestId, dto.Comment);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpPut("{requestId:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid requestId,
        [FromBody] RejectVolunteerRequestDto dto,
        [FromServices] RejectVolunteerRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized();

        var command = new RejectVolunteerRequestCommand(adminId.Value, requestId, dto.Comment);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpPut("{requestId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid requestId,
        [FromServices] ApproveVolunteerRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized();

        var command = new ApproveVolunteerRequestCommand(adminId.Value, requestId);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpPut("{requestId:guid}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> UpdateRequest(
        [FromRoute] Guid requestId,
        [FromBody] UpdateVolunteerRequestDto dto,
        [FromServices] UpdateVolunteerRequestHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new UpdateVolunteerRequestCommand(
            userId.Value,
            requestId,
            new VolunteerInfo(dto.Experience, dto.Certificates, dto.Requisites));

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpGet("unreviewed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUnreviewed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromServices] GetUnreviewedRequestsHandler? handler = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUnreviewedRequestsQuery(page, pageSize);
        var result = await handler!.Handle(query, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByAdmin(
        [FromQuery] VolunteerRequestStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromServices] GetRequestsByAdminHandler? handler = null,
        CancellationToken cancellationToken = default)
    {
        var adminId = GetUserId();
        if (adminId is null) return Unauthorized();

        var query = new GetRequestsByAdminQuery(adminId.Value, status, page, pageSize);
        var result = await handler!.Handle(query, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }

    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetByUser(
        [FromQuery] VolunteerRequestStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromServices] GetRequestsByUserHandler? handler = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetRequestsByUserQuery(userId.Value, status, page, pageSize);
        var result = await handler!.Handle(query, cancellationToken);
        return result.IsSuccess ? this.ToOkResponse(result.Value) : result.Error.ToResponse();
    }
}