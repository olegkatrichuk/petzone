namespace PetZone.Volunteers.Application.News;

public record CreateNewsPostCommand(Guid VolunteerId, string Title, string Content);