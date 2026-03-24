using PetZone.VolunteerRequests.Domain;
using Xunit;

namespace PetZone.Domain.Tests.VolunteerRequests;

public class DiscussionTests
{
    private static readonly Guid User1 = Guid.NewGuid();
    private static readonly Guid User2 = Guid.NewGuid();

    private static Discussion CreateDiscussion() =>
        Discussion.Create(Guid.NewGuid(), [User1, User2]).Value;

    // Create
    [Fact]
    public void Create_ShouldSucceed_WithTwoParticipants()
    {
        var result = Discussion.Create(Guid.NewGuid(), [User1, User2]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Users.Count);
        Assert.False(result.Value.IsClosed);
    }

    [Fact]
    public void Create_ShouldFail_WithOneParticipant()
    {
        var result = Discussion.Create(Guid.NewGuid(), [User1]);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_ShouldFail_WithThreeParticipants()
    {
        var result = Discussion.Create(Guid.NewGuid(), [User1, User2, Guid.NewGuid()]);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_ShouldFail_WithDuplicateParticipants()
    {
        var result = Discussion.Create(Guid.NewGuid(), [User1, User1]);

        Assert.True(result.IsFailure);
    }

    // AddMessage
    [Fact]
    public void AddMessage_ShouldSucceed_WhenParticipantAddsMessage()
    {
        var discussion = CreateDiscussion();

        var result = discussion.AddMessage(User1, "Hello!");

        Assert.True(result.IsSuccess);
        Assert.Single(discussion.Messages);
    }

    [Fact]
    public void AddMessage_ShouldFail_WhenNonParticipantAddsMessage()
    {
        var discussion = CreateDiscussion();

        var result = discussion.AddMessage(Guid.NewGuid(), "Hello!");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AddMessage_ShouldFail_WhenDiscussionIsClosed()
    {
        var discussion = CreateDiscussion();
        discussion.Close();

        var result = discussion.AddMessage(User1, "Hello!");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AddMessage_ShouldFail_WhenTextIsEmpty()
    {
        var discussion = CreateDiscussion();

        var result = discussion.AddMessage(User1, "");

        Assert.True(result.IsFailure);
    }

    // DeleteMessage
    [Fact]
    public void DeleteMessage_ShouldSucceed_WhenOwnerDeletesMessage()
    {
        var discussion = CreateDiscussion();
        discussion.AddMessage(User1, "Hello!");
        var messageId = discussion.Messages[0].Id;

        var result = discussion.DeleteMessage(User1, messageId);

        Assert.True(result.IsSuccess);
        Assert.Empty(discussion.Messages);
    }

    [Fact]
    public void DeleteMessage_ShouldFail_WhenNonOwnerDeletesMessage()
    {
        var discussion = CreateDiscussion();
        discussion.AddMessage(User1, "Hello!");
        var messageId = discussion.Messages[0].Id;

        var result = discussion.DeleteMessage(User2, messageId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DeleteMessage_ShouldFail_WhenMessageNotFound()
    {
        var discussion = CreateDiscussion();

        var result = discussion.DeleteMessage(User1, Guid.NewGuid());

        Assert.True(result.IsFailure);
    }

    // EditMessage
    [Fact]
    public void EditMessage_ShouldSucceed_WhenOwnerEditsMessage()
    {
        var discussion = CreateDiscussion();
        discussion.AddMessage(User1, "Hello!");
        var messageId = discussion.Messages[0].Id;

        var result = discussion.EditMessage(User1, messageId, "Updated text");

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated text", discussion.Messages[0].Text);
        Assert.True(discussion.Messages[0].IsEdited);
    }

    [Fact]
    public void EditMessage_ShouldFail_WhenNonOwnerEditsMessage()
    {
        var discussion = CreateDiscussion();
        discussion.AddMessage(User1, "Hello!");
        var messageId = discussion.Messages[0].Id;

        var result = discussion.EditMessage(User2, messageId, "Updated text");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void EditMessage_ShouldFail_WhenDiscussionIsClosed()
    {
        var discussion = CreateDiscussion();
        discussion.AddMessage(User1, "Hello!");
        var messageId = discussion.Messages[0].Id;
        discussion.Close();

        var result = discussion.EditMessage(User1, messageId, "Updated text");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void EditMessage_ShouldFail_WhenNewTextIsEmpty()
    {
        var discussion = CreateDiscussion();
        discussion.AddMessage(User1, "Hello!");
        var messageId = discussion.Messages[0].Id;

        var result = discussion.EditMessage(User1, messageId, "");

        Assert.True(result.IsFailure);
    }

    // Close
    [Fact]
    public void Close_ShouldSucceed_WhenDiscussionIsOpen()
    {
        var discussion = CreateDiscussion();

        var result = discussion.Close();

        Assert.True(result.IsSuccess);
        Assert.True(discussion.IsClosed);
    }

    [Fact]
    public void Close_ShouldFail_WhenAlreadyClosed()
    {
        var discussion = CreateDiscussion();
        discussion.Close();

        var result = discussion.Close();

        Assert.True(result.IsFailure);
    }
}