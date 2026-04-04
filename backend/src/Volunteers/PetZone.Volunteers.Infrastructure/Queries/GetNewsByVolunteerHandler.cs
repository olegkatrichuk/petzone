using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetNewsByVolunteerHandler(
    VolunteersDbContext dbContext,
    ILogger<GetNewsByVolunteerHandler> logger)
{
    public async Task<List<NewsPostDto>> Handle(
        Guid volunteerId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting news for volunteer {VolunteerId}", volunteerId);

        return await dbContext.NewsPosts
            .Where(n => n.VolunteerId == volunteerId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NewsPostDto(n.Id, n.VolunteerId, n.Title, n.Content, n.CreatedAt, n.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
