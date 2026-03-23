using Microsoft.AspNetCore.Authorization;

namespace PetZone.Accounts.Infrastructure.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}