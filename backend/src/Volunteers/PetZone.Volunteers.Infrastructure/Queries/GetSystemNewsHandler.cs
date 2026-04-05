using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetSystemNewsHandler(VolunteersDbContext db)
{
    public async Task<List<SystemNewsPostDto>> Handle(int page, int pageSize, CancellationToken ct)
    {
        return await db.SystemNewsPosts
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new SystemNewsPostDto(p.Id, p.Title, p.Content, p.Type, p.PublishedAt))
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct) =>
        await db.SystemNewsPosts.CountAsync(ct);
}