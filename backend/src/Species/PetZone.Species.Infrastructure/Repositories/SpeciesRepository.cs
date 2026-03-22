using Microsoft.EntityFrameworkCore;
using SpeciesEntity = PetZone.Species.Domain.Species;

namespace PetZone.Species.Infrastructure.Repositories;

public class SpeciesRepository(SpeciesDbContext dbContext)
{
    public async Task<SpeciesEntity?> GetByIdAsync(Guid speciesId, CancellationToken cancellationToken = default)
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