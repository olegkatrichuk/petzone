using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class CreateVolunteerService(
    IVolunteerRepository repository,
    IValidator<CreateVolunteerCommand> validator,
    ILogger<CreateVolunteerService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateVolunteerCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling CreateVolunteerCommand");

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage))
                .ToList();

            logger.LogWarning("Validation failed with {Count} errors", errors.Count);
            return new ErrorList(errors);
        }

        var req = command.Request;

        var email = Email.Create(req.Email).Value;
        var fullName = FullName.Create(req.FirstName, req.LastName, req.Patronymic).Value;
        var experience = Experience.Create(req.ExperienceYears).Value;
        var phone = PhoneNumber.Create(req.Phone).Value;

        var volunteerResult = Volunteer.Create(
            Guid.NewGuid(), fullName, email,
            req.GeneralDescription, experience, phone);

        if (volunteerResult.IsFailure)
            return (ErrorList)volunteerResult.Error;

        var volunteer = volunteerResult.Value;

        foreach (var sn in req.SocialNetworks)
            volunteer.AddSocialNetwork(SocialNetwork.Create(sn.Name, sn.Link).Value);

        foreach (var r in req.Requisites)
            volunteer.AddRequisite(Requisite.Create(r.Name, r.Description).Value);

        await repository.AddAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer saved to database. Id: {VolunteerId}", volunteer.Id);

        return volunteer.Id;
    }
}