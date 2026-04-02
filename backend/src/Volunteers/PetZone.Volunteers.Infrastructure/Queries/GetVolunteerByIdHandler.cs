using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetVolunteerByIdHandler(
    VolunteersDbContext dbContext,
    ICacheService cacheService,
    ILogger<GetVolunteerByIdHandler> logger)
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public async Task<Result<VolunteerDto, ErrorList>> Handle(
        GetVolunteerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting volunteer by id {VolunteerId}", query.VolunteerId);

        var cacheKey = $"volunteer:{query.VolunteerId}";

        var volunteer = await cacheService.GetOrSetAsync<VolunteerDto>(
            cacheKey,
            CacheOptions,
            async () => await dbContext.Volunteers
                .Where(v => !v.IsDeleted && v.Id == query.VolunteerId)
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
                .FirstOrDefaultAsync(cancellationToken),
            cancellationToken);

        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", query.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        return volunteer;
    }
}