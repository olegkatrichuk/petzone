using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Repositories;

public interface IDiscussionRepository
{
    Task AddAsync(Discussion discussion, CancellationToken cancellationToken = default);
    Task<Discussion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Discussion?> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default);
}