namespace PetZone.Accounts.Domain;

public class AdminAccount
{
    public const string RoleName = "Admin";

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}