namespace PetZone.Volunteers.Application.News;

public record DeleteNewsPostCommand(Guid NewsPostId, Guid RequestingVolunteerId);