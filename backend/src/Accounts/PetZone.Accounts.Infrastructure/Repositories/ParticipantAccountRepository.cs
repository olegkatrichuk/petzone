
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Infrastructure.Repositories;

public class ParticipantAccountRepository(AccountsDbContext dbContext) 
    : IParticipantAccountRepository
{
    public async Task AddAsync(
        ParticipantAccount account, 
        CancellationToken cancellationToken = default)
    {
        await dbContext.ParticipantAccounts.AddAsync(account, cancellationToken);
    }
}