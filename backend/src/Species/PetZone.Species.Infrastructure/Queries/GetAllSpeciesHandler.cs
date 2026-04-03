using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Species.Application.Queries;
using PetZone.Species.Contracts;

namespace PetZone.Species.Infrastructure.Queries;

public class GetAllSpeciesHandler(
    SpeciesDbContext dbContext,
    ICacheService cache,
    ILogger<GetAllSpeciesHandler> logger)
{
    public async Task<Result<IReadOnlyList<SpeciesDto>, ErrorList>> Handle(
        GetAllSpeciesQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"species:all:{query.Locale}";

        var cached = await cache.GetOrSetAsync<List<SpeciesDto>>(
            cacheKey,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            async () =>
            {
                logger.LogInformation("Getting all species from DB");
                var allSpecies = await dbContext.Species
                    .Include(s => s.Breeds)
                    .ToListAsync(cancellationToken);

                return allSpecies
                    .OrderBy(s => s.GetName(query.Locale))
                    .Select(s => new SpeciesDto(
                        s.Id,
                        s.GetName(query.Locale),
                        s.Breeds.Count,
                        s.Translations))
                    .ToList();
            },
            cancellationToken);

        return cached ?? [];
    }
}