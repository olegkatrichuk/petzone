using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using PetZone.Listings.Application;

namespace PetZone.Listings.Infrastructure;

public class ListingsUnitOfWork(ListingsDbContext dbContext) : IListingsUnitOfWork
{
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        return transaction.GetDbTransaction();
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
