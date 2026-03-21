using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Contracts.Species;
using PetZone.Domain.Shared;
using PetZone.UseCases.Queries;

namespace PetZone.Infrastructure.Queries;

public class GetBreedsBySpeciesIdHandler(
    ReadDbContext dbContext,
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
            .OrderBy(b => b.Name)
            .Select(b => new BreedDto(b.Id, b.Name))
            .ToList();

        return breeds;
    }
}