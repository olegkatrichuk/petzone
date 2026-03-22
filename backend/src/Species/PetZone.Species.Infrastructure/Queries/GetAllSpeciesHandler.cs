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