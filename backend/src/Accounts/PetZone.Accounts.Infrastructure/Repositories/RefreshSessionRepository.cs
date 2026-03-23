using Microsoft.EntityFrameworkCore;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Infrastructure.Repositories;

public class RefreshSessionRepository(AccountsDbContext dbContext) : IRefreshSessionRepository
{
    public async Task AddAsync(
        RefreshSession session,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshSessions.AddAsync(session, cancellationToken);
    }

    public async Task<RefreshSession?> GetByRefreshTokenAsync(
        Guid refreshToken,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshSessions
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task DeleteAsync(
        RefreshSession session,
        CancellationToken cancellationToken = default)
    {
        dbContext.RefreshSessions.Remove(session);
    }
}