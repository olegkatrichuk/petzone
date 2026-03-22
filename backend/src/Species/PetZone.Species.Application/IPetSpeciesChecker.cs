namespace PetZone.Species.Application;

public interface IPetSpeciesChecker
{
    Task<bool> HasPetsWithSpeciesAsync(Guid speciesId, CancellationToken cancellationToken = default);
    Task<bool> HasPetsWithBreedAsync(Guid breedId, CancellationToken cancellationToken = default);
}