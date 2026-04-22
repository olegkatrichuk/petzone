using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public record PagedList<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public class GetVolunteersHandler(
    VolunteersDbContext dbContext,
    ILogger<GetVolunteersHandler> logger)
{
    public async Task<Result<PagedList<VolunteerDto>, ErrorList>> Handle(
        GetVolunteersQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting volunteers. Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        // PostgreSQL has no MIN(uuid), so fetch lightweight rows and deduplicate in memory.
        // Ordered by last_name + first_name so DistinctBy keeps the alphabetically first first name.
        var lightweight = await dbContext.Volunteers
            .Where(v => !v.IsDeleted && !v.IsSystem)
            .OrderBy(v => v.Name.LastName)
            .ThenBy(v => v.Name.FirstName)
            .Select(v => new { v.Id, LastName = v.Name.LastName })
            .ToListAsync(cancellationToken);

        var deduplicatedIds = lightweight
            .DistinctBy(v => v.LastName)
            .Select(v => v.Id)
            .ToList();

        var totalCount = deduplicatedIds.Count;

        var pageIds = deduplicatedIds
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var volunteers = await dbContext.Volunteers
            .Where(v => pageIds.Contains(v.Id))
            .OrderBy(v => v.Name.LastName)
            .Select(v => new VolunteerDto(
                v.Id,
                v.Name.FirstName,
                v.Name.LastName,
                v.Name.Patronymic,
                v.Email.Value,
                v.Phone.Value,
                v.Experience.Years,
                v.GeneralDescription,
                v.Pets.Count(p => !p.IsDeleted),
                v.IsDeleted,
                v.PhotoPath,
                v.SocialNetworks.Select(s => new SocialNetworkDto(s.Name, s.Link)).ToList(),
                v.Requisites.Select(r => new RequisiteDto(r.Name, r.Description)).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedList<VolunteerDto>(volunteers, totalCount, query.Page, query.PageSize);
    }
}
