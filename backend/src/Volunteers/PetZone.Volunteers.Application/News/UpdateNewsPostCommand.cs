namespace PetZone.Volunteers.Application.News;

public record UpdateNewsPostCommand(Guid NewsPostId, Guid RequestingVolunteerId, string Title, string Content);