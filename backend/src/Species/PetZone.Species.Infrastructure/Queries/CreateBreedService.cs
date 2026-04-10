using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Species.Application.Commands;
using PetZone.Species.Domain;
using PetZone.Species.Infrastructure.Repositories;

namespace PetZone.Species.Infrastructure.Queries;

public class CreateBreedService(
    SpeciesDbContext dbContext,
    SpeciesRepository speciesRepository,
    ICacheService cache,
    ILogger<CreateBreedService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateBreedCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating breed for species {SpeciesId}", command.SpeciesId);

        var species = await speciesRepository.GetByIdAsync(command.SpeciesId, cancellationToken);
        if (species is null)
            return (ErrorList)Error.NotFound("species.not_found", "Вид не найден.");

        var breedResult = Breed.Create(Guid.NewGuid(), command.Translations);
        if (breedResult.IsFailure)
            return (ErrorList)breedResult.Error;

        var addResult = species.AddBreed(breedResult.Value);
        if (addResult.IsFailure)
            return (ErrorList)addResult.Error;

        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByPrefixAsync($"species:{command.SpeciesId}", cancellationToken);
        await cache.RemoveByPrefixAsync("species:all", cancellationToken);

        logger.LogInformation("Breed {BreedId} created for species {SpeciesId}",
            breedResult.Value.Id, command.SpeciesId);

        return breedResult.Value.Id;
    }
}