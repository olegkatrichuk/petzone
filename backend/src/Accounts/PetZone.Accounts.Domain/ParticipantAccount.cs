namespace PetZone.Accounts.Domain;

public class ParticipantAccount
{
    public const string RoleName = "Participant";

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public List<Guid> FavoritePets { get; set; } = [];
}