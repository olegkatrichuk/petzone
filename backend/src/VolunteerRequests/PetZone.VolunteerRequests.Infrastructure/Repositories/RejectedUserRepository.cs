using Microsoft.EntityFrameworkCore;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure.Repositories;

public class RejectedUserRepository(VolunteerRequestsDbContext dbContext)
    : IRejectedUserRepository
{
    public async Task AddAsync(RejectedUser rejectedUser, CancellationToken cancellationToken = default)
    {
        await dbContext.RejectedUsers.AddAsync(rejectedUser, cancellationToken);
    }

    public async Task<RejectedUser?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.RejectedUsers
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
    }
}