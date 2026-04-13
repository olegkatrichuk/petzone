using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Events;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Application.Commands;

public class CreateAdoptionApplicationHandler(
    IAdoptionApplicationRepository applicationRepository,
    IVolunteerRepository volunteerRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<CreateAdoptionApplicationHandler> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateAdoptionApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", $"Volunteer {command.VolunteerId} not found.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId && !p.IsDeleted);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", $"Pet {command.PetId} not found.");

        var alreadyApplied = await applicationRepository.ExistsAsync(
            command.PetId, command.ApplicantUserId, cancellationToken);
        if (alreadyApplied)
            return (ErrorList)Error.Conflict("adoption.already_applied",
                "You have already submitted an application for this pet.");

        var applicationResult = AdoptionApplication.Create(
            command.PetId,
            command.VolunteerId,
            command.ApplicantUserId,
            command.ApplicantName,
            command.ApplicantPhone,
            command.Message);

        if (applicationResult.IsFailure)
            return (ErrorList)applicationResult.Error;

        var application = applicationResult.Value;

        await applicationRepository.AddAsync(application, cancellationToken);

        await publishEndpoint.Publish(new AdoptionApplicationCreatedEvent(
            ApplicationId: application.Id,
            PetId: pet.Id,
            PetNickname: pet.Nickname,
            VolunteerId: volunteer.Id,
            VolunteerEmail: volunteer.Email.Value,
            VolunteerName: $"{volunteer.Name.FirstName} {volunteer.Name.LastName}",
            ApplicantName: command.ApplicantName,
            ApplicantPhone: command.ApplicantPhone,
            Message: command.Message), cancellationToken);

        logger.LogInformation("Adoption application {ApplicationId} created for pet {PetId}", application.Id, pet.Id);

        return application.Id;
    }
}