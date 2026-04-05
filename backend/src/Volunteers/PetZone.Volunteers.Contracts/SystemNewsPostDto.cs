namespace PetZone.Volunteers.Contracts;

public record SystemNewsPostDto(
    Guid Id,
    string Title,
    string Content,
    string Type,
    DateTime PublishedAt);