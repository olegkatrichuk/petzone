using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Species.Application;
using PetZone.Species.Application.Commands;

namespace PetZone.Species.Infrastructure.Queries;

public class DeleteSpeciesService(
    SpeciesDbContext dbContext,
    IPetSpeciesChecker petSpeciesChecker,
    ILogger<DeleteSpeciesService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteSpeciesCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting species {SpeciesId}", command.SpeciesId);

        var hasPets = await petSpeciesChecker.HasPetsWithSpeciesAsync(command.SpeciesId, cancellationToken);

        if (hasPets)
            return (ErrorList)Error.Conflict(
                "species.has_pets",
                "Нельзя удалить вид — у некоторых животных указан этот вид.");

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