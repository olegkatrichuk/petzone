using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PetZone.VolunteerRequests.Application;

namespace PetZone.VolunteerRequests.Infrastructure;

public class VolunteerRequestsUnitOfWork(VolunteerRequestsDbContext dbContext)
    : IVolunteerRequestsUnitOfWork
{
    public async Task<IDbTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return transaction.GetDbTransaction();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}