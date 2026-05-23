using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Application.Repositories;

public interface IBlogPostRepository
{
    Task<Guid> AddAsync(BlogPost post, CancellationToken ct = default);
    Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<(IReadOnlyList<BlogPost> Items, int Total)> GetPagedAsync(
        string? language, int page, int pageSize, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task DeleteAsync(BlogPost post, CancellationToken ct = default);
}
