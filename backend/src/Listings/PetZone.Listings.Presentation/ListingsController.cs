using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetZone.Listings.Application.Commands.CreateListing;
using PetZone.Listings.Application.Commands.DeleteListing;
using PetZone.Listings.Application.Commands.AddListingPhoto;
using PetZone.Listings.Application.Commands.MarkAdopted;
using PetZone.Listings.Application.Commands.RemoveListingPhoto;
using PetZone.Listings.Application.Commands.UpdateListing;
using PetZone.Listings.Application.Queries;
using PetZone.Listings.Contracts;
using PetZone.Listings.Infrastructure.Queries;
using PetZone.Listings.Infrastructure.Services;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Listings.Presentation;

[ApiController]
[Route("[controller]")]
public class ListingsController(
    CreateListingService createListingService,
    UpdateListingService updateListingService,
    DeleteListingService deleteListingService,
    MarkAdoptedService markAdoptedService,
    AddListingPhotoService addListingPhotoService,
    RemoveListingPhotoService removeListingPhotoService,
    GetAllListingsHandler getAllListingsHandler,
    GetListingByIdHandler getListingByIdHandler,
    GetMyListingsHandler getMyListingsHandler) : ControllerBase
{
    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] Guid? speciesId = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await getAllListingsHandler.Handle(
            new GetAllListingsQuery(speciesId, city, search, page, pageSize), ct);
        return this.ToOkResponse(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await getListingByIdHandler.Handle(new GetListingByIdQuery(id), ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(result.Value);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult> GetMy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await getMyListingsHandler.Handle(
            new GetMyListingsQuery(userId.Value, page, pageSize), ct);
        return this.ToOkResponse(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateListingRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var firstName = User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value ?? string.Empty;
        var lastName = User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value ?? string.Empty;
        var userName = $"{firstName} {lastName}".Trim();
        if (string.IsNullOrWhiteSpace(userName)) userName = "Користувач";

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? string.Empty;

        var command = new CreateListingCommand(
            userId.Value, userName, userEmail, request.Phone, request.ContactEmail,
            request.Title, request.Description, request.SpeciesId, request.BreedId,
            request.AgeMonths, request.Color, request.City,
            request.Vaccinated, request.Castrated);

        var result = await createListingService.Handle(command, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(new { id = result.Value });
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateListingRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new UpdateListingCommand(
            id, userId.Value,
            request.Title, request.Description, request.SpeciesId, request.BreedId,
            request.AgeMonths, request.Color, request.City,
            request.Vaccinated, request.Castrated, request.Phone, request.ContactEmail);

        var result = await updateListingService.Handle(command, ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(null);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await deleteListingService.Handle(
            new DeleteListingCommand(id, userId.Value), ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(null);
    }

    [Authorize]
    [HttpPost("{id:guid}/photos")]
    public async Task<ActionResult> AddPhoto(
        [FromRoute] Guid id,
        [FromBody] AddPhotoRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await addListingPhotoService.Handle(
            new AddListingPhotoCommand(id, userId.Value, request.FileName), ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(null);
    }

    [Authorize]
    [HttpDelete("{id:guid}/photos/{fileName}")]
    public async Task<ActionResult> RemovePhoto(
        [FromRoute] Guid id,
        [FromRoute] string fileName,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await removeListingPhotoService.Handle(
            new RemoveListingPhotoCommand(id, userId.Value, fileName), ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(null);
    }

    [Authorize]
    [HttpPatch("{id:guid}/adopted")]
    public async Task<ActionResult> MarkAdopted(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await markAdoptedService.Handle(
            new MarkAdoptedCommand(id, userId.Value), ct);
        if (result.IsFailure)
            return result.Error.ToResponse();
        return this.ToOkResponse(null);
    }
}
