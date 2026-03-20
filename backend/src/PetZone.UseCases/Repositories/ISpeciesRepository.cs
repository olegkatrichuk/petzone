using PetZone.Domain.Species;

namespace PetZone.UseCases.Repositories;

public interface ISpeciesRepository
{
    Task<Species?> GetByIdAsync(Guid speciesId, CancellationToken cancellationToken = default);
    
    Task<bool> BreedExistsAsync(Guid speciesId, Guid breedId, CancellationToken cancellationToken = default);
}