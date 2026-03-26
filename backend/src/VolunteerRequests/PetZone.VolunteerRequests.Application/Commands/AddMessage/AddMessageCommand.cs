namespace PetZone.VolunteerRequests.Application.Commands.AddMessage;

public record AddMessageCommand(
    Guid UserId,
    Guid DiscussionId,
    string Text
);