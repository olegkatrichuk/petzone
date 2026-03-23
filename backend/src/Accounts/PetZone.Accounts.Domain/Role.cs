using Microsoft.AspNetCore.Identity;

namespace PetZone.Accounts.Domain;

public class Role : IdentityRole<Guid>
{
    public const string Admin = "Admin";
    public const string Volunteer = "Volunteer";
    public const string Participant = "Participant";

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}