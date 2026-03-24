namespace PetZone.VolunteerRequests.Application.Commands.TakeOnReview;

public record TakeOnReviewCommand(
    Guid AdminId,
    Guid RequestId
);