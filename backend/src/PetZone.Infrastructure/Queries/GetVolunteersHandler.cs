using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Contracts.Volunteers;
using PetZone.Domain.Shared;
using PetZone.Infrastructure;
using PetZone.UseCases.Queries;

namespace PetZone.UseCases.Volunteers;

public record PagedList<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public class GetVolunteersHandler(
    ReadDbContext dbContext,
    ILogger<GetVolunteersHandler> logger)
{
    public async Task<Result<PagedList<VolunteerDto>, ErrorList>> Handle(
        GetVolunteersQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting volunteers. Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var volunteersQuery = dbContext.Volunteers
            .Where(v => !v.IsDeleted)
            .OrderBy(v => v.Name.LastName);

        var totalCount = await volunteersQuery.CountAsync(cancellationToken);

        var volunteers = await volunteersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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
                v.IsDeleted))
            .ToListAsync(cancellationToken);

        return new PagedList<VolunteerDto>(volunteers, totalCount, query.Page, query.PageSize);
    }
}