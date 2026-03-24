using System.Data;

namespace PetZone.VolunteerRequests.Application;

public interface IVolunteerRequestsUnitOfWork
{
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}