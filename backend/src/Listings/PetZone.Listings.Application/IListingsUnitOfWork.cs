using System.Data;

namespace PetZone.Listings.Application;

public interface IListingsUnitOfWork
{
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
