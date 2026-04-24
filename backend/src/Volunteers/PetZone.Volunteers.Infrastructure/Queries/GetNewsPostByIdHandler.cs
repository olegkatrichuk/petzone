using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetNewsPostByIdHandler(VolunteersDbContext dbContext)
{
    public async Task<NewsPostDto?> Handle(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.NewsPosts
            .Where(n => n.Id == id)
            .Select(n => new NewsPostDto(n.Id, n.VolunteerId, n.Title, n.Content, n.CreatedAt, n.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
}
