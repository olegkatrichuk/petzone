using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.VolunteerRequests.Domain;

public class Message
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsEdited { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Message() { }

    public static Result<Message, Error> Create(Guid userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Error.Validation("message.empty_text", "Message text cannot be empty.");

        return new Message
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Text = text,
            IsEdited = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public UnitResult<Error> Edit(Guid userId, string newText)
    {
        if (UserId != userId)
            return Error.Forbidden("message.forbidden", "You can only edit your own messages.");

        if (string.IsNullOrWhiteSpace(newText))
            return Error.Validation("message.empty_text", "Message text cannot be empty.");

        Text = newText;
        IsEdited = true;
        return UnitResult.Success<Error>();
    }
}
