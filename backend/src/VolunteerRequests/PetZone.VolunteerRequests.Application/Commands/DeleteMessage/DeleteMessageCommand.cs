namespace PetZone.VolunteerRequests.Application.Commands.DeleteMessage;

public record DeleteMessageCommand(
    Guid UserId,
    Guid DiscussionId,
    Guid MessageId
);