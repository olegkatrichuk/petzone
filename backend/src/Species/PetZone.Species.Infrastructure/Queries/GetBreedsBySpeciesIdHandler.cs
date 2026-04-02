using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Species.Application.Queries;
using PetZone.Species.Contracts;

namespace PetZone.Species.Infrastructure.Queries;

public class GetBreedsBySpeciesIdHandler(
    SpeciesDbContext dbContext,
    ILogger<GetBreedsBySpeciesIdHandler> logger)
{
    public async Task<Result<IReadOnlyList<BreedDto>, ErrorList>> Handle(
        GetBreedsBySpeciesIdQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting breeds for species {SpeciesId}", query.SpeciesId);

        var species = await dbContext.Species
            .Include(s => s.Breeds)
            .FirstOrDefaultAsync(s => s.Id == query.SpeciesId, cancellationToken);

        if (species is null)
            return (ErrorList)Error.NotFound("species.not_found", "Вид не найден.");

        var breeds = species.Breeds
            .OrderBy(b => b.GetName(query.Locale))
            .Select(b => new BreedDto(b.Id, b.GetName(query.Locale), b.Translations))
            .ToList();

        return breeds;
    }
}