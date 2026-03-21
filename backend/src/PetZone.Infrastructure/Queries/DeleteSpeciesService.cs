using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;

namespace PetZone.Infrastructure.Queries;

public class DeleteSpeciesService(
    ApplicationDbContext dbContext,
    ReadDbContext readDbContext,
    ILogger<DeleteSpeciesService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteSpeciesCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting species {SpeciesId}", command.SpeciesId);

        // 1. Проверяем через ReadDbContext есть ли животные с этим видом
        var hasPets = await readDbContext.Volunteers
            .SelectMany(v => v.Pets)
            .AnyAsync(p => p.SpeciesBreedInfo.SpeciesId == command.SpeciesId, cancellationToken);

        if (hasPets)
            return (ErrorList)Error.Conflict(
                "species.has_pets",
                "Нельзя удалить вид — у некоторых животных указан этот вид.");

        // 2. Находим вид через WriteDbContext
        var species = await dbContext.Species
            .FirstOrDefaultAsync(s => s.Id == command.SpeciesId, cancellationToken);

        if (species is null)
            return (ErrorList)Error.NotFound("species.not_found", "Вид не найден.");

        dbContext.Species.Remove(species);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Species {SpeciesId} deleted", command.SpeciesId);

        return command.SpeciesId;
    }
}