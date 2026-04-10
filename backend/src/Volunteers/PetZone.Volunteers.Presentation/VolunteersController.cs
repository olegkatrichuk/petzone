using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Application.Commands;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Application.Volunteers;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Infrastructure.Queries;
using PetZone.Volunteers.Presentation.Extensions;
using PetZone.Accounts.Infrastructure.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;

namespace PetZone.Volunteers.Presentation;

[ApiController]
[Route("[controller]")]
public class VolunteersController(
    CreateVolunteerService createVolunteerService,
    UpdateVolunteerMainInfoService updateMainInfoService,
    UpdateVolunteerSocialNetworksService updateSocialNetworksService,
    UpdateVolunteerRequisitesService updateRequisitesService,
    DeleteVolunteerService deleteVolunteerService,
    HardDeleteVolunteerService hardDeleteVolunteerService,
    UploadVolunteerPhotoService uploadVolunteerPhotoService,
    GetVolunteersHandler getVolunteersHandler,
    GetVolunteerByIdHandler getVolunteerByIdHandler,
    ILogger<VolunteersController> logger) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;
    
    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    [Authorize(Policy = Permissions.Volunteers.Create)]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        logger.LogInformation("Creating volunteer. Email: {Email}", request.Email);
        var result = await createVolunteerService.Handle(request.ToCommand(userId.Value), cancellationToken);
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
    
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = Math.Clamp(pageSize, 1, 100);

        logger.LogInformation("Getting volunteers. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetVolunteersQuery(page, pageSize);
        var result = await getVolunteersHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }
    
    [Authorize(Policy = Permissions.Volunteers.Update)]
    [HttpPost("{id:guid}/photo")]
    public async Task<ActionResult> UploadPhoto(
        [FromRoute] Guid id,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null || !Guid.TryParse(claim.Value, out var requesterId) || requesterId != id)
            return Forbid();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest($"Invalid file type: {extension}");
        if (file.Length > MaxFileSize)
            return BadRequest("File exceeds 5MB limit");

        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream, cancellationToken);

        const int maxDim = 4000;
        if (image.Width > maxDim || image.Height > maxDim)
        {
            var ratio = Math.Min((double)maxDim / image.Width, (double)maxDim / image.Height);
            image.Mutate(ctx => ctx.Resize((int)(image.Width * ratio), (int)(image.Height * ratio)));
        }

        var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, new WebpEncoder { Quality = 85 }, cancellationToken);
        outputStream.Position = 0;

        var fileName = $"volunteers/{id}/{Guid.NewGuid()}.webp";
        var result = await uploadVolunteerPhotoService.Handle(id, outputStream, fileName, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [AllowAnonymous]
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