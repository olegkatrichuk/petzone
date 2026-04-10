using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Species.Application.Commands;
using SpeciesEntity = PetZone.Species.Domain.Species;

namespace PetZone.Species.Infrastructure.Queries;

public class CreateSpeciesService(
    SpeciesDbContext dbContext,
    ICacheService cache,
    ILogger<CreateSpeciesService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateSpeciesCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating species");

        var speciesResult = SpeciesEntity.Create(Guid.NewGuid(), command.Translations);
        if (speciesResult.IsFailure)
            return (ErrorList)speciesResult.Error;

        var species = speciesResult.Value;

        var exists = await dbContext.Species.AnyAsync(
            s => s.Translations == species.Translations,
            cancellationToken);

        if (exists)
            return (ErrorList)Error.Conflict("species.already_exists", "Вид с таким названием уже существует.");

        dbContext.Species.Add(species);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByPrefixAsync("species:all", cancellationToken);

        logger.LogInformation("Species {SpeciesId} created", species.Id);
        return species.Id;
    }
}