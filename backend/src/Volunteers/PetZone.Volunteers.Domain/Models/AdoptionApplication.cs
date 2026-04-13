using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models;

public enum AdoptionApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public class AdoptionApplication
{
    public Guid Id { get; private set; }
    public Guid PetId { get; private set; }
    public Guid VolunteerId { get; private set; }
    public Guid ApplicantUserId { get; private set; }
    public string ApplicantName { get; private set; } = null!;
    public string ApplicantPhone { get; private set; } = null!;
    public string? Message { get; private set; }
    public AdoptionApplicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AdoptionApplication() { }

    public static Result<AdoptionApplication, Error> Create(
        Guid petId,
        Guid volunteerId,
        Guid applicantUserId,
        string applicantName,
        string applicantPhone,
        string? message)
    {
        if (petId == Guid.Empty)
            return Error.Validation("adoption.invalid_pet", "PetId is required.");

        if (volunteerId == Guid.Empty)
            return Error.Validation("adoption.invalid_volunteer", "VolunteerId is required.");

        if (string.IsNullOrWhiteSpace(applicantPhone))
            return Error.Validation("adoption.invalid_phone", "Phone is required.");

        return new AdoptionApplication
        {
            Id = Guid.NewGuid(),
            PetId = petId,
            VolunteerId = volunteerId,
            ApplicantUserId = applicantUserId,
            ApplicantName = applicantName,
            ApplicantPhone = applicantPhone,
            Message = message,
            Status = AdoptionApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public UnitResult<Error> Approve()
    {
        if (Status != AdoptionApplicationStatus.Pending)
            return Error.Conflict("adoption.invalid_status", $"Cannot approve from status {Status}.");

        Status = AdoptionApplicationStatus.Approved;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Reject()
    {
        if (Status != AdoptionApplicationStatus.Pending)
            return Error.Conflict("adoption.invalid_status", $"Cannot reject from status {Status}.");

        Status = AdoptionApplicationStatus.Rejected;
        return UnitResult.Success<Error>();
    }
}