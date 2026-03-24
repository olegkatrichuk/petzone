using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.VolunteerRequests.Domain;

public class Discussion
{
    private readonly List<Message> _messages = [];
    private readonly List<Guid> _users = [];

    public Guid Id { get; private set; }
    public Guid RelationId { get; private set; }
    public IReadOnlyList<Guid> Users => _users.AsReadOnly();
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();
    public bool IsClosed { get; private set; }

    private Discussion() { }

    public static Result<Discussion, Error> Create(Guid relationId, List<Guid> users)
    {
        if (users is null || users.Count != 2)
            return Error.Validation("discussion.invalid_users",
                "Discussion must have exactly 2 participants.");

        if (users.Distinct().Count() != 2)
            return Error.Validation("discussion.duplicate_users",
                "Discussion participants must be unique.");

        var discussion = new Discussion
        {
            Id = Guid.NewGuid(),
            RelationId = relationId,
            IsClosed = false
        };
        discussion._users.AddRange(users);

        return discussion;
    }

    public UnitResult<Error> AddMessage(Guid userId, string text)
    {
        if (IsClosed)
            return Error.Conflict("discussion.closed", "Cannot add message to closed discussion.");

        if (!_users.Contains(userId))
            return Error.Forbidden("discussion.forbidden", "User is not a participant of this discussion.");

        var messageResult = Message.Create(userId, text);
        if (messageResult.IsFailure)
            return messageResult.Error;

        _messages.Add(messageResult.Value);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> DeleteMessage(Guid userId, Guid messageId)
    {
        if (IsClosed)
            return Error.Conflict("discussion.closed", "Cannot delete message from closed discussion.");

        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message is null)
            return Error.NotFound("message.not_found", $"Message {messageId} not found.");

        if (message.UserId != userId)
            return Error.Forbidden("message.forbidden", "You can only delete your own messages.");

        _messages.Remove(message);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> EditMessage(Guid userId, Guid messageId, string newText)
    {
        if (IsClosed)
            return Error.Conflict("discussion.closed", "Cannot edit message in closed discussion.");

        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message is null)
            return Error.NotFound("message.not_found", $"Message {messageId} not found.");

        return message.Edit(userId, newText);
    }

    public UnitResult<Error> Close()
    {
        if (IsClosed)
            return Error.Conflict("discussion.already_closed", "Discussion is already closed.");

        IsClosed = true;
        return UnitResult.Success<Error>();
    }
}