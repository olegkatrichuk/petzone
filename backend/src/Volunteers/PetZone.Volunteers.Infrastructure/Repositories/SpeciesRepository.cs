using Microsoft.EntityFrameworkCore;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Application.Repositories;

namespace PetZone.Volunteers.Infrastructure.Repositories;

public class SpeciesRepository(SpeciesDbContext dbContext) : ISpeciesRepository
{
    public async Task<PetZone.Species.Domain.Species?> GetByIdAsync(Guid speciesId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Species
            .Include(s => s.Breeds)
            .FirstOrDefaultAsync(s => s.Id == speciesId, cancellationToken);
    }

    public async Task<bool> BreedExistsAsync(Guid speciesId, Guid breedId, CancellationToken cancellationToken = default)
    {
        var species = await GetByIdAsync(speciesId, cancellationToken);
        if (species is null) return false;

        return species.Breeds.Any(b => b.Id == breedId);
    }
}