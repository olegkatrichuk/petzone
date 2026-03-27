using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Domain;
using StackExchange.Redis;

namespace PetZone.Accounts.Infrastructure.Repositories;

public class RedisRefreshSessionRepository(
    IConnectionMultiplexer multiplexer,
    UserManager<User> userManager) : IRefreshSessionRepository
{
    private static string GetKey(Guid refreshToken) => $"refresh_session:{refreshToken}";

    public async Task AddAsync(RefreshSession session, CancellationToken cancellationToken = default)
    {
        var db = multiplexer.GetDatabase();
        var data = JsonSerializer.Serialize(new RefreshSessionData(
            session.Id,
            session.UserId,
            session.RefreshToken,
            session.Jti,
            session.CreatedAt,
            session.ExpiresAt));

        var ttl = session.ExpiresAt - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
            await db.StringSetAsync(GetKey(session.RefreshToken), data, ttl);
    }

    public async Task<RefreshSession?> GetByRefreshTokenAsync(
        Guid refreshToken,
        CancellationToken cancellationToken = default)
    {
        var db = multiplexer.GetDatabase();
        var raw = await db.StringGetAsync(GetKey(refreshToken));

        if (raw.IsNullOrEmpty)
            return null;

        var stored = JsonSerializer.Deserialize<RefreshSessionData>((string)raw!);
        if (stored is null)
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

    public async Task DeleteAsync(RefreshSession session, CancellationToken cancellationToken = default)
    {
        var db = multiplexer.GetDatabase();
        await db.KeyDeleteAsync(GetKey(session.RefreshToken));
    }

    private record RefreshSessionData(
        Guid Id,
        Guid UserId,
        Guid RefreshToken,
        Guid Jti,
        DateTime CreatedAt,
        DateTime ExpiresAt);
}
