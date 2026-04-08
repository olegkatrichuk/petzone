using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure;

public class VolunteerAccountService(
    IVolunteerRepository repository,
    ILogger<VolunteerAccountService> logger) : IVolunteerAccountService
{
    public async Task<Result<Guid, Error>> CreateAsync(
        Guid userId,
        string firstName,
        string lastName,
        string email,
        string? phone,
        int experienceYears,
        string description,
        CancellationToken cancellationToken = default)
    {
        var fullNameResult = FullName.Create(firstName, lastName);
        if (fullNameResult.IsFailure) return fullNameResult.Error;

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure) return emailResult.Error;

        var experienceResult = Experience.Create(experienceYears);
        if (experienceResult.IsFailure) return experienceResult.Error;

        var phoneResult = PhoneNumber.Create(phone ?? "0000000000");
        if (phoneResult.IsFailure) return phoneResult.Error;

        var volunteerResult = Volunteer.Create(
            Guid.NewGuid(), userId,
            fullNameResult.Value, emailResult.Value,
            description, experienceResult.Value, phoneResult.Value);

        if (volunteerResult.IsFailure) return volunteerResult.Error;

        await repository.AddAsync(volunteerResult.Value, cancellationToken);

        logger.LogInformation("Volunteer account created for user {UserId}", userId);

        return volunteerResult.Value.Id;
    }
}
