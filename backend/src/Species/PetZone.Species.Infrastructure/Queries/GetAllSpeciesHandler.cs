using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Species.Application.Queries;
using PetZone.Species.Contracts;

namespace PetZone.Species.Infrastructure.Queries;

public class GetAllSpeciesHandler(
    SpeciesDbContext dbContext,
    ILogger<GetAllSpeciesHandler> logger)
{
    public async Task<Result<IReadOnlyList<SpeciesDto>, ErrorList>> Handle(
        GetAllSpeciesQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all species");

        var allSpecies = await dbContext.Species
            .Include(s => s.Breeds)
            .ToListAsync(cancellationToken);

        var species = allSpecies
            .OrderBy(s => s.GetName(query.Locale))
            .Select(s => new SpeciesDto(
                s.Id,
                s.GetName(query.Locale),
                s.Breeds.Count,
                s.Translations))
            .ToList();

        return species;
    }
}