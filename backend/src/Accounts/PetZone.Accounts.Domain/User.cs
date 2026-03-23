using Microsoft.AspNetCore.Identity;

namespace PetZone.Accounts.Domain;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public ParticipantAccount? ParticipantAccount { get; set; }
    public VolunteerAccount? VolunteerAccount { get; set; }
    public AdminAccount? AdminAccount { get; set; }
    

    public static User CreateAdmin(string email, string firstName, string lastName, Role adminRole)
    {
        if (adminRole.Name != AdminAccount.RoleName)
            throw new InvalidOperationException("Role is not admin");

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        };
        
        return user;
    }

    public static User CreateParticipant(string email, string firstName, string lastName, Role participantRole)
    {
        if (participantRole.Name != ParticipantAccount.RoleName)
            throw new InvalidOperationException("Role is not participant");

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        };
        
        return user;
    }
}