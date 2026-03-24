namespace PetZone.VolunteerRequests.Domain;

public class RejectedUser
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime RejectedUntil { get; private set; }

    private RejectedUser() { }

    public static RejectedUser Create(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        RejectedUntil = DateTime.UtcNow.AddDays(7)
    };

    public bool IsBlocked() => DateTime.UtcNow < RejectedUntil;
}