using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using PetZone.Core;

namespace PetZone.Accounts.Infrastructure.Authorization;

public class PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirst("sub");

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var cacheKey = $"user:{userId}:permissions";

        var permissions = await cache.GetOrSetAsync<string[]>(
            cacheKey,
            CacheOptions,
            async () =>
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
                return await dbContext.RolePermissions
                    .Where(rp => dbContext.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rp.RoleId))
                    .Select(rp => rp.Permission.Code)
                    .ToArrayAsync();
            });

        if (permissions is not null && permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}