using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Species.Application;
using PetZone.Species.Application.Commands;

namespace PetZone.Species.Infrastructure.Queries;

public class DeleteBreedService(
    SpeciesDbContext dbContext,
    IPetSpeciesChecker petSpeciesChecker,
    ILogger<DeleteBreedService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteBreedCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting breed {BreedId} from species {SpeciesId}",
            command.BreedId, command.SpeciesId);

        var hasPets = await petSpeciesChecker.HasPetsWithBreedAsync(command.BreedId, cancellationToken);

        if (hasPets)
            return (ErrorList)Error.Conflict(
                "breed.has_pets",
                "Нельзя удалить породу — у некоторых животных указана эта порода.");

        var species = await dbContext.Species
            .Include(s => s.Breeds)
            .FirstOrDefaultAsync(s => s.Id == command.SpeciesId, cancellationToken);

        if (species is null)
            return (ErrorList)Error.NotFound("species.not_found", "Вид не найден.");

        var breed = species.Breeds.FirstOrDefault(b => b.Id == command.BreedId);
        if (breed is null)
            return (ErrorList)Error.NotFound("breed.not_found", "Порода не найдена.");

        species.RemoveBreed(breed);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Breed {BreedId} deleted from species {SpeciesId}",
            command.BreedId, command.SpeciesId);

        return command.BreedId;
    }
}