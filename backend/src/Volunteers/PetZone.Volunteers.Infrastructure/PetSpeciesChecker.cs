using Microsoft.EntityFrameworkCore;
using PetZone.Species.Application;

namespace PetZone.Volunteers.Infrastructure;

public class PetSpeciesChecker(VolunteersDbContext dbContext) : IPetSpeciesChecker
{
    public async Task<bool> HasPetsWithSpeciesAsync(Guid speciesId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Volunteers
            .SelectMany(v => v.Pets)
            .AnyAsync(p => p.SpeciesBreedInfo.SpeciesId == speciesId, cancellationToken);
    }

    public async Task<bool> HasPetsWithBreedAsync(Guid breedId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Volunteers
            .SelectMany(v => v.Pets)
            .AnyAsync(p => p.SpeciesBreedInfo.BreedId == breedId, cancellationToken);
    }
}