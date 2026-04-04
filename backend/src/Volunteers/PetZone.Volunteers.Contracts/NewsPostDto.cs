namespace PetZone.Volunteers.Contracts;

public record NewsPostDto(
    Guid Id,
    Guid VolunteerId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
