using Microsoft.EntityFrameworkCore;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure.Repositories;

public class DiscussionRepository(VolunteerRequestsDbContext dbContext)
    : IDiscussionRepository
{
    public async Task AddAsync(Discussion discussion, CancellationToken cancellationToken = default)
    {
        await dbContext.Discussions.AddAsync(discussion, cancellationToken);
    }

    public async Task<Discussion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Discussions
            .Include(d => d.Messages)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Discussion?> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Discussions
            .Include(d => d.Messages)
            .FirstOrDefaultAsync(d => d.RelationId == relationId, cancellationToken);
    }
}