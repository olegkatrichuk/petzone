using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetSystemNewsHandler(VolunteersDbContext db)
{
    private static SystemNewsPostDto ToDto(Domain.Models.SystemNewsPost p) =>
        new(p.Id, p.Type, p.PublishedAt,
            p.LookingForHome, p.NeedsHelp, p.FoundHomeThisWeek,
            p.TotalVolunteers, p.FactEn,
            p.TopBreedsJson, p.TopCity,
            p.FeaturedPetNickname, p.FeaturedPetPhotoUrl,
            p.FeaturedPetDescription, p.FeaturedPetBreed, p.FeaturedPetCity);

    public async Task<SystemNewsPostDto?> GetTodayAsync(CancellationToken ct) =>
        await db.SystemNewsPosts
            .OrderByDescending(p => p.PublishedAt)
            .Select(p => ToDto(p))
            .FirstOrDefaultAsync(ct);

    public async Task<List<SystemNewsPostDto>> Handle(int page, int pageSize, CancellationToken ct) =>
        await db.SystemNewsPosts
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

    public async Task<int> CountAsync(CancellationToken ct) =>
        await db.SystemNewsPosts.CountAsync(ct);
}