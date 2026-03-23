using Microsoft.AspNetCore.Identity;

namespace PetZone.Accounts.Domain;

public class Role : IdentityRole<Guid>
{
    public const string Admin = AdminAccount.RoleName;
    public const string Volunteer = VolunteerAccount.RoleName;
    public const string Participant = ParticipantAccount.RoleName;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}