using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models;

public class SpeciesBreed : ValueObject
{
    public Guid SpeciesId { get; }
    public Guid BreedId { get; }

    public SpeciesBreed(Guid speciesId, Guid breedId)
    {
        if (speciesId == Guid.Empty) throw new ArgumentException("SpeciesId не может быть пустым.");
        if (breedId == Guid.Empty) throw new ArgumentException("BreedId не может быть пустым.");

        SpeciesId = speciesId;
        BreedId = breedId;
    }

    private SpeciesBreed() { } // Для EF Core

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SpeciesId;
        yield return BreedId;
    }
}