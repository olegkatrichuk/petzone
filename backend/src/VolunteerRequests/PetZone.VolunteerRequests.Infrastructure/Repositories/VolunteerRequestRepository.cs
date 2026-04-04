using Microsoft.EntityFrameworkCore;
using PetZone.VolunteerRequests.Application.Queries.GetStats;
using PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;
using PetZone.VolunteerRequests.Application.Repositories;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Infrastructure.Repositories;

public class VolunteerRequestRepository(VolunteerRequestsDbContext dbContext)
    : IVolunteerRequestRepository
{
    public async Task AddAsync(VolunteerRequest request, CancellationToken cancellationToken = default)
    {
        await dbContext.VolunteerRequests.AddAsync(request, cancellationToken);
    }

    public async Task<VolunteerRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.VolunteerRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<VolunteerRequest?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VolunteerRequests
            .FirstOrDefaultAsync(r => r.UserId == userId &&
                r.Status != VolunteerRequestStatus.Rejected &&
                r.Status != VolunteerRequestStatus.Approved,
                cancellationToken);
    }

    public async Task<PagedResult<VolunteerRequest>> GetUnreviewedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.VolunteerRequests
            .Where(r => r.Status == VolunteerRequestStatus.Submitted);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<VolunteerRequest>(items, total, page, pageSize);
    }

    public async Task<PagedResult<VolunteerRequest>> GetByAdminAsync(
        Guid adminId, VolunteerRequestStatus status, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.VolunteerRequests
            .Where(r => r.AdminId == adminId && r.Status == status);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<VolunteerRequest>(items, total, page, pageSize);
    }

    public async Task<PagedResult<VolunteerRequest>> GetByUserAsync(
        Guid userId, VolunteerRequestStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.VolunteerRequests.Where(r => r.UserId == userId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<VolunteerRequest>(items, total, page, pageSize);
    }

    public async Task<VolunteerRequestStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await dbContext.VolunteerRequests
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int Get(VolunteerRequestStatus s) => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

        return new VolunteerRequestStats(
            Total: counts.Sum(c => c.Count),
            Submitted: Get(VolunteerRequestStatus.Submitted),
            OnReview: Get(VolunteerRequestStatus.OnReview),
            RevisionRequired: Get(VolunteerRequestStatus.RevisionRequired),
            Approved: Get(VolunteerRequestStatus.Approved),
            Rejected: Get(VolunteerRequestStatus.Rejected)
        );
    }
}