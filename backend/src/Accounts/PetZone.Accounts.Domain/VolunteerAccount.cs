namespace PetZone.Accounts.Domain;

public class VolunteerAccount
{
    public const string RoleName = "Volunteer";

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public int Experience { get; set; }
    public List<string> Certificates { get; set; } = [];
    public List<string> Requisites { get; set; } = [];
}