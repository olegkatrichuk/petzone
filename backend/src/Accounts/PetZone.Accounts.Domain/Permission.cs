namespace PetZone.Accounts.Domain;

public class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}