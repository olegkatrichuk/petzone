using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Core;

public interface IVolunteerAccountService
{
    Task<Result<Guid, Error>> CreateAsync(
        Guid userId,
        string firstName,
        string lastName,
        string email,
        string? phone,
        int experienceYears,
        string description,
        CancellationToken cancellationToken = default);
}
