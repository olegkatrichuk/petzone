using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Repositories;

public class NewsPostRepository(VolunteersDbContext dbContext) : INewsPostRepository
{
    public async Task<Guid> AddAsync(NewsPost newsPost, CancellationToken cancellationToken = default)
    {
        await dbContext.NewsPosts.AddAsync(newsPost, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newsPost.Id;
    }

    public async Task<NewsPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.NewsPosts.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(NewsPost newsPost, CancellationToken cancellationToken = default)
    {
        dbContext.NewsPosts.Remove(newsPost);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}