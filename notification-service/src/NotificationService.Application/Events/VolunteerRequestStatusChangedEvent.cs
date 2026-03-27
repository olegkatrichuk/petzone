namespace NotificationService.Application.Events;

public record VolunteerRequestStatusChangedEvent(
    Guid UserId,
    Guid RequestId,
    string Status,
    string? Comment
);