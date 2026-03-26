namespace PetZone.VolunteerRequests.Application.Commands.EditMessage;

public record EditMessageCommand(
    Guid UserId,
    Guid DiscussionId,
    Guid MessageId,
    string NewText
);