using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class UpdateVolunteerMainInfoService(
    IVolunteerRepository repository,
    IValidator<UpdateVolunteerMainInfoCommand> validator,
    ILogger<UpdateVolunteerMainInfoService> logger)
{
    public async Task<Result<Guid, Error>> Handle(
        UpdateVolunteerMainInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating main info for volunteer {VolunteerId}", command.VolunteerId);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var first = validationResult.Errors.First();
            return Error.Validation(first.ErrorCode, first.ErrorMessage);
        }

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        var req = command.Request;
        var email = Email.Create(req.Email).Value;
        var fullName = FullName.Create(req.FirstName, req.LastName, req.Patronymic).Value;
        var experience = Experience.Create(req.ExperienceYears).Value;
        var phone = PhoneNumber.Create(req.Phone).Value;

        var result = volunteer.UpdateMainInfo(fullName, email, req.GeneralDescription, experience, phone);
        if (result.IsFailure)
            return result.Error;

        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} main info updated successfully", volunteer.Id);

        return volunteer.Id;
    }
}