namespace PetZone.Volunteers.Application.Repositories;

public interface ISpeciesRepository
{
    Task<PetZone.Species.Domain.Species?> GetByIdAsync(Guid speciesId, CancellationToken cancellationToken = default);
    
    Task<bool> BreedExistsAsync(Guid speciesId, Guid breedId, CancellationToken cancellationToken = default);
}