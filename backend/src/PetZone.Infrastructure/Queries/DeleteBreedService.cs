using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;

namespace PetZone.Infrastructure.Queries;

public class DeleteBreedService(
    ApplicationDbContext dbContext,
    ReadDbContext readDbContext,
    ILogger<DeleteBreedService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteBreedCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting breed {BreedId} from species {SpeciesId}",
            command.BreedId, command.SpeciesId);

        // 1. Проверяем через ReadDbContext есть ли животные с этой породой
        var hasPets = await readDbContext.Volunteers
            .SelectMany(v => v.Pets)
            .AnyAsync(p => p.SpeciesBreedInfo.BreedId == command.BreedId, cancellationToken);

        if (hasPets)
            return (ErrorList)Error.Conflict(
                "breed.has_pets",
                "Нельзя удалить породу — у некоторых животных указана эта порода.");

        // 2. Находим вид с породами
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