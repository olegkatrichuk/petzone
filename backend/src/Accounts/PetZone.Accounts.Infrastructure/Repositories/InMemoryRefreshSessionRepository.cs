using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Infrastructure.Repositories;

public class InMemoryRefreshSessionRepository(
    IMemoryCache cache,
    UserManager<User> userManager) : IRefreshSessionRepository
{
    private static string GetKey(Guid refreshToken) => $"refresh_session:{refreshToken}";

    public Task AddAsync(RefreshSession session, CancellationToken cancellationToken = default)
    {
        var ttl = session.ExpiresAt - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
            return Task.CompletedTask;

        var data = new RefreshSessionData(
            session.Id,
            session.UserId,
            session.RefreshToken,
            session.Jti,
            session.CreatedAt,
            session.ExpiresAt);

        cache.Set(GetKey(session.RefreshToken), data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });

        return Task.CompletedTask;
    }

    public async Task<RefreshSession?> GetByRefreshTokenAsync(
        Guid refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (!cache.TryGetValue(GetKey(refreshToken), out RefreshSessionData? stored) || stored is null)
            return null;

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
            return null;

        return new RefreshSession
        {
            Id = stored.Id,
            UserId = stored.UserId,
            User = user,
            RefreshToken = stored.RefreshToken,
            Jti = stored.Jti,
            CreatedAt = stored.CreatedAt,
            ExpiresAt = stored.ExpiresAt
        };
    }

    public Task DeleteAsync(RefreshSession session, CancellationToken cancellationToken = default)
    {
        cache.Remove(GetKey(session.RefreshToken));
        return Task.CompletedTask;
    }

    private record RefreshSessionData(
        Guid Id,
        Guid UserId,
        Guid RefreshToken,
        Guid Jti,
        DateTime CreatedAt,
        DateTime ExpiresAt);
}