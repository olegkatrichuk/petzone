namespace PetZone.Accounts.Domain;

public class RefreshSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RefreshToken { get; set; }
    public Guid Jti { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}