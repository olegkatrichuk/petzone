using CSharpFunctionalExtensions;
using PetZone.VolunteerRequests.Domain;
using Xunit;

namespace PetZone.Domain.Tests.VolunteerRequests;

public class VolunteerRequestTests
{
    private static VolunteerInfo CreateVolunteerInfo() => new(
        Experience: 2,
        Certificates: ["Certificate 1"],
        Requisites: ["Requisite 1"]
    );

    // Create
    [Fact]
    public void Create_ShouldSucceed_WithValidData()
    {
        var userId = Guid.NewGuid();
        var info = CreateVolunteerInfo();

        var result = VolunteerRequest.Create(userId, info);

        Assert.True(result.IsSuccess);
        Assert.Equal(VolunteerRequestStatus.Submitted, result.Value.Status);
        Assert.Equal(userId, result.Value.UserId);
    }

    [Fact]
    public void Create_ShouldFail_WhenVolunteerInfoIsNull()
    {
        var result = VolunteerRequest.Create(Guid.NewGuid(), null!);

        Assert.True(result.IsFailure);
    }

    // TakeOnReview
    [Fact]
    public void TakeOnReview_ShouldSucceed_FromSubmitted()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        var adminId = Guid.NewGuid();

        var result = request.TakeOnReview(adminId);

        Assert.True(result.IsSuccess);
        Assert.Equal(VolunteerRequestStatus.OnReview, request.Status);
        Assert.Equal(adminId, request.AdminId);
    }

    [Fact]
    public void TakeOnReview_ShouldFail_FromApproved()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());
        request.Approve(Guid.NewGuid());

        var result = request.TakeOnReview(Guid.NewGuid());

        Assert.True(result.IsFailure);
    }

    // SendForRevision
    [Fact]
    public void SendForRevision_ShouldSucceed_FromOnReview()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());

        var result = request.SendForRevision("Please fix your info");

        Assert.True(result.IsSuccess);
        Assert.Equal(VolunteerRequestStatus.RevisionRequired, request.Status);
        Assert.Equal("Please fix your info", request.RejectionComment);
    }

    [Fact]
    public void SendForRevision_ShouldFail_WhenCommentIsEmpty()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());

        var result = request.SendForRevision("");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void SendForRevision_ShouldFail_FromSubmitted()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;

        var result = request.SendForRevision("comment");

        Assert.True(result.IsFailure);
    }

    // Reject
    [Fact]
    public void Reject_ShouldSucceed_FromOnReview()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());

        var result = request.Reject("Not qualified");

        Assert.True(result.IsSuccess);
        Assert.Equal(VolunteerRequestStatus.Rejected, request.Status);
        Assert.Equal("Not qualified", request.RejectionComment);
    }

    [Fact]
    public void Reject_ShouldFail_WhenCommentIsEmpty()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());

        var result = request.Reject("");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Reject_ShouldFail_FromSubmitted()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;

        var result = request.Reject("comment");

        Assert.True(result.IsFailure);
    }

    // Approve
    [Fact]
    public void Approve_ShouldSucceed_FromOnReview()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());
        var discussionId = Guid.NewGuid();

        var result = request.Approve(discussionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(VolunteerRequestStatus.Approved, request.Status);
        Assert.Equal(discussionId, request.DiscussionId);
    }

    [Fact]
    public void Approve_ShouldFail_FromSubmitted()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;

        var result = request.Approve(Guid.NewGuid());

        Assert.True(result.IsFailure);
    }

    // TakeOnReview from RevisionRequired
    [Fact]
    public void TakeOnReview_ShouldSucceed_FromRevisionRequired()
    {
        var request = VolunteerRequest.Create(Guid.NewGuid(), CreateVolunteerInfo()).Value;
        request.TakeOnReview(Guid.NewGuid());
        request.SendForRevision("Fix this");

        var result = request.TakeOnReview(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(VolunteerRequestStatus.OnReview, request.Status);
    }
}