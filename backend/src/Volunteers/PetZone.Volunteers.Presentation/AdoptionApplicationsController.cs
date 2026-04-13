using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetZone.Accounts.Infrastructure.Authorization;
using PetZone.Volunteers.Application.Commands;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Domain.Models;
using PetZone.Volunteers.Presentation.Extensions;
using System.Security.Claims;

namespace PetZone.Volunteers.Presentation;

[Authorize]
[ApiController]
[Route("adoption-applications")]
public class AdoptionApplicationsController(
    CreateAdoptionApplicationHandler createHandler,
    UpdateApplicationStatusHandler updateStatusHandler,
    IAdoptionApplicationRepository repository,
    IVolunteerRepository volunteerRepository) : ControllerBase
{
    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    /// <summary>Submit an adoption application for a pet.</summary>
    [Authorize(Policy = Permissions.Adoptions.Create)]
    [HttpPost("pets/{petId:guid}/volunteers/{volunteerId:guid}")]
    public async Task<ActionResult> Create(
        Guid petId,
        Guid volunteerId,
        [FromBody] CreateAdoptionApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CreateAdoptionApplicationCommand(
            PetId: petId,
            VolunteerId: volunteerId,
            ApplicantUserId: userId.Value,
            ApplicantName: request.ApplicantName,
            ApplicantPhone: request.ApplicantPhone,
            Message: request.Message);

        var result = await createHandler.Handle(command, cancellationToken);
        if (result.IsFailure) return result.Error.ToResponse();

        return this.ToOkResponse(new { id = result.Value });
    }

    /// <summary>Get all applications for the current user (applicant view).</summary>
    [Authorize(Policy = Permissions.Adoptions.Read)]
    [HttpGet("my")]
    public async Task<ActionResult> GetMyApplications(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var applications = await repository.GetByApplicantIdAsync(userId.Value, cancellationToken);
        var dtos = await MapToDtosAsync(applications, cancellationToken);

        return this.ToOkResponse(dtos);
    }

    /// <summary>Get all applications for a volunteer's pets (volunteer view).</summary>
    [Authorize(Policy = Permissions.Adoptions.Read)]
    [HttpGet("volunteer/{volunteerId:guid}")]
    public async Task<ActionResult> GetByVolunteer(Guid volunteerId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var volunteer = await volunteerRepository.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer is null) return NotFound();

        if (volunteer.UserId != userId.Value && !User.IsInRole("Admin"))
            return Forbid();

        var applications = await repository.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        var dtos = await MapToDtosAsync(applications, cancellationToken);

        return this.ToOkResponse(dtos);
    }

    /// <summary>Approve or reject an adoption application.</summary>
    [Authorize(Policy = Permissions.Adoptions.UpdateStatus)]
    [HttpPatch("{applicationId:guid}/status")]
    public async Task<ActionResult> UpdateStatus(
        Guid applicationId,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var application = await repository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null) return NotFound();

        var volunteer = await volunteerRepository.GetByIdAsync(application.VolunteerId, cancellationToken);
        if (volunteer is null) return NotFound();

        if (volunteer.UserId != userId.Value && !User.IsInRole("Admin"))
            return Forbid();

        var command = new UpdateApplicationStatusCommand(
            ApplicationId: applicationId,
            VolunteerId: application.VolunteerId,
            Action: request.Action);

        var result = await updateStatusHandler.Handle(command, cancellationToken);
        if (result.IsFailure) return result.Error.ToResponse();

        return this.ToOkResponse(null);
    }

    private async Task<IReadOnlyList<AdoptionApplicationDto>> MapToDtosAsync(
        IReadOnlyList<AdoptionApplication> applications,
        CancellationToken cancellationToken)
    {
        var dtos = new List<AdoptionApplicationDto>();

        foreach (var app in applications)
        {
            var volunteer = await volunteerRepository.GetByIdAsync(app.VolunteerId, cancellationToken);
            var pet = volunteer?.Pets.FirstOrDefault(p => p.Id == app.PetId);
            var mainPhoto = pet?.Photos.FirstOrDefault(p => p.IsMain)?.FilePath
                         ?? pet?.Photos.FirstOrDefault()?.FilePath;

            dtos.Add(new AdoptionApplicationDto(
                Id: app.Id,
                PetId: app.PetId,
                PetNickname: pet?.Nickname ?? "",
                PetMainPhoto: mainPhoto,
                VolunteerId: app.VolunteerId,
                ApplicantUserId: app.ApplicantUserId,
                ApplicantName: app.ApplicantName,
                ApplicantPhone: app.ApplicantPhone,
                Message: app.Message,
                Status: app.Status.ToString(),
                CreatedAt: app.CreatedAt));
        }

        return dtos;
    }
}
