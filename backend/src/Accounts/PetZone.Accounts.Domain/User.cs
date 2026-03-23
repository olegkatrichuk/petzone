using Microsoft.AspNetCore.Identity;

namespace PetZone.Accounts.Domain;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public ParticipantAccount? ParticipantAccount { get; set; }
    public VolunteerAccount? VolunteerAccount { get; set; }
    public AdminAccount? AdminAccount { get; set; }
}