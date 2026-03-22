using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace PetZone.Volunteers.Application.Volunteers;

public class UpdateVolunteerMainInfoService(
    IVolunteerRepository repository,
    IValidator<UpdateVolunteerMainInfoCommand> validator,
    ILogger<UpdateVolunteerMainInfoService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateVolunteerMainInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating main info for volunteer {VolunteerId}", command.VolunteerId);

        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage))
                .ToList();

            logger.LogWarning("Validation failed with {Count} errors", errors.Count);
            return new ErrorList(errors);
        }

        var volunteer = await repository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", command.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        var req = command.Request;
        var email = Email.Create(req.Email).Value;
        var fullName = FullName.Create(req.FirstName, req.LastName, req.Patronymic).Value;
        var experience = Experience.Create(req.ExperienceYears).Value;
        var phone = PhoneNumber.Create(req.Phone).Value;

        var result = volunteer.UpdateMainInfo(fullName, email, req.GeneralDescription, experience, phone);
        if (result.IsFailure)
            return (ErrorList)result.Error;

        await repository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Volunteer {VolunteerId} main info updated successfully", volunteer.Id);

        return volunteer.Id;
    }
}