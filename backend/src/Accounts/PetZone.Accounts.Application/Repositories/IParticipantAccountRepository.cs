using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Application.Repositories;

public interface IParticipantAccountRepository
{
    Task AddAsync(ParticipantAccount account, CancellationToken cancellationToken = default);
}