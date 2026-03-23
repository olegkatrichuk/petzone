using PetZone.Accounts.Application;

namespace PetZone.Accounts.Infrastructure;

public class AccountsUnitOfWork(AccountsDbContext dbContext) : IAccountsUnitOfWork
{
    public async Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}