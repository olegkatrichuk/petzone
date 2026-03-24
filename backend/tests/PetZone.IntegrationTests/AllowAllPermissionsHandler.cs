using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PetZone.IntegrationTests;

public class AllowAllAuthorizationService : IAuthorizationService
{
    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
        => Task.FromResult(AuthorizationResult.Success());

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, object? resource, string policyName)
        => Task.FromResult(AuthorizationResult.Success());
}
