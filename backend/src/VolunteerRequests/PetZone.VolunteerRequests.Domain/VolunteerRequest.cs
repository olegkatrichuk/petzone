using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.VolunteerRequests.Domain;

public class VolunteerRequest
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? AdminId { get; private set; }
    public Guid? DiscussionId { get; private set; }
    public VolunteerInfo VolunteerInfo { get; private set; } = null!;
    public VolunteerRequestStatus Status { get; private set; }
    public string? RejectionComment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VolunteerRequest() { }

    public static Result<VolunteerRequest, Error> Create(
        Guid userId,
        VolunteerInfo volunteerInfo)
    {
        if (volunteerInfo is null)
            return Error.Validation("volunteer_request.invalid_info", "VolunteerInfo is required.");

        return new VolunteerRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VolunteerInfo = volunteerInfo,
            Status = VolunteerRequestStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };
    }

    public UnitResult<Error> TakeOnReview(Guid adminId, Guid discussionId)
    {
        if (Status != VolunteerRequestStatus.Submitted &&
            Status != VolunteerRequestStatus.RevisionRequired)
            return Error.Conflict("volunteer_request.invalid_status",
                $"Cannot take on review from status {Status}.");

        AdminId = adminId;
        DiscussionId = discussionId;
        Status = VolunteerRequestStatus.OnReview;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SendForRevision(string rejectionComment)
    {
        if (Status != VolunteerRequestStatus.OnReview)
            return Error.Conflict("volunteer_request.invalid_status",
                $"Cannot send for revision from status {Status}.");

        if (string.IsNullOrWhiteSpace(rejectionComment))
            return Error.Validation("volunteer_request.empty_comment",
                "Rejection comment is required.");

        Status = VolunteerRequestStatus.RevisionRequired;
        RejectionComment = rejectionComment;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Approve()
    {
        if (Status != VolunteerRequestStatus.OnReview)
            return Error.Conflict("volunteer_request.invalid_status",
                $"Cannot approve from status {Status}.");

        Status = VolunteerRequestStatus.Approved;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AutoApprove()
    {
        if (Status != VolunteerRequestStatus.Submitted)
            return Error.Conflict("volunteer_request.invalid_status",
                $"Cannot auto-approve from status {Status}.");

        Status = VolunteerRequestStatus.Approved;
        return UnitResult.Success<Error>();
    }
    public UnitResult<Error> Reject(string rejectionComment)
    {
        if (Status != VolunteerRequestStatus.OnReview)
            return Error.Conflict("volunteer_request.invalid_status",
                $"Cannot reject from status {Status}.");

        if (string.IsNullOrWhiteSpace(rejectionComment))
            return Error.Validation("volunteer_request.empty_comment",
                "Rejection comment is required.");

        Status = VolunteerRequestStatus.Rejected;
        RejectionComment = rejectionComment;
        return UnitResult.Success<Error>();
    }
    public UnitResult<Error> Update(VolunteerInfo volunteerInfo)
    {
        if (Status != VolunteerRequestStatus.RevisionRequired)
            return Error.Conflict("volunteer_request.invalid_status",
                $"Cannot update request from status {Status}.");

        if (volunteerInfo is null)
            return Error.Validation("volunteer_request.invalid_info", "VolunteerInfo is required.");

        VolunteerInfo = volunteerInfo;
        Status = VolunteerRequestStatus.Submitted;
        return UnitResult.Success<Error>();
    }
}