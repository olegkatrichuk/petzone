using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Species.Application.Queries;
using PetZone.Species.Contracts;

namespace PetZone.Species.Infrastructure.Queries;

public class GetBreedsBySpeciesIdHandler(
    SpeciesDbContext dbContext,
    ICacheService cache,
    ILogger<GetBreedsBySpeciesIdHandler> logger)
{
    public async Task<Result<IReadOnlyList<BreedDto>, ErrorList>> Handle(
        GetBreedsBySpeciesIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"species:{query.SpeciesId}:breeds:{query.Locale}";

        var cached = await cache.GetOrSetAsync<List<BreedDto>>(
            cacheKey,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            async () =>
            {
                logger.LogInformation("Getting breeds for species {SpeciesId} from DB", query.SpeciesId);

                var species = await dbContext.Species
                    .Include(s => s.Breeds)
                    .FirstOrDefaultAsync(s => s.Id == query.SpeciesId, cancellationToken);

                if (species is null) return null;

                return species.Breeds
                    .OrderBy(b => b.GetName(query.Locale))
                    .Select(b => new BreedDto(b.Id, b.GetName(query.Locale), b.Translations))
                    .ToList();
            },
            cancellationToken);

        if (cached is null)
            return (ErrorList)Error.NotFound("species.not_found", "Вид не найден.");

        return cached;
    }
}