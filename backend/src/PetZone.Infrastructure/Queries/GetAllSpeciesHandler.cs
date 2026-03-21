using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Contracts.Species;
using PetZone.Domain.Shared;
using PetZone.UseCases.Queries;

namespace PetZone.Infrastructure.Queries;

public class GetAllSpeciesHandler(
    ReadDbContext dbContext,
    ILogger<GetAllSpeciesHandler> logger)
{
    public async Task<Result<IReadOnlyList<SpeciesDto>, ErrorList>> Handle(
        GetAllSpeciesQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all species");

        var species = await dbContext.Species
            .OrderBy(s => s.Name)
            .Select(s => new SpeciesDto(
                s.Id,
                s.Name,
                s.Breeds.Count))
            .ToListAsync(cancellationToken);

        return species;
    }
}