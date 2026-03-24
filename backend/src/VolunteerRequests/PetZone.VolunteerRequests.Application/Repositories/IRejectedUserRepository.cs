using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Repositories;

public interface IRejectedUserRepository
{
    Task AddAsync(RejectedUser rejectedUser, CancellationToken cancellationToken = default);
    Task<RejectedUser?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}