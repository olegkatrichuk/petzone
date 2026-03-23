namespace PetZone.Accounts.Application;

public interface IAccountsUnitOfWork
{
    Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}