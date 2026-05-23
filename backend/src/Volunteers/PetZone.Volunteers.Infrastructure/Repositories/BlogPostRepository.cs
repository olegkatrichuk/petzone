using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Repositories;

public class BlogPostRepository(VolunteersDbContext dbContext) : IBlogPostRepository
{
    public async Task<Guid> AddAsync(BlogPost post, CancellationToken ct = default)
    {
        await dbContext.BlogPosts.AddAsync(post, ct);
        await dbContext.SaveChangesAsync(ct);
        return post.Id;
    }

    public Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.BlogPosts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        dbContext.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        dbContext.BlogPosts.AnyAsync(p => p.Slug == slug, ct);

    public async Task<(IReadOnlyList<BlogPost> Items, int Total)> GetPagedAsync(
        string? language, int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.BlogPosts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(language))
            query = query.Where(p => p.Language == language);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public Task SaveAsync(CancellationToken ct = default) =>
        dbContext.SaveChangesAsync(ct);

    public async Task DeleteAsync(BlogPost post, CancellationToken ct = default)
    {
        dbContext.BlogPosts.Remove(post);
        await dbContext.SaveChangesAsync(ct);
    }
}
