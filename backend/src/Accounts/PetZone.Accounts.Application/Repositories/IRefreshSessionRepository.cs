using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Application.Repositories;

public interface IRefreshSessionRepository
{
    Task AddAsync(RefreshSession session, CancellationToken cancellationToken = default);
    Task<RefreshSession?> GetByRefreshTokenAsync(Guid refreshToken, CancellationToken cancellationToken = default);
    Task DeleteAsync(RefreshSession session, CancellationToken cancellationToken = default);
}