using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models;

public class SpeciesBreed : ValueObject
{
    public Guid SpeciesId { get; }
    public Guid BreedId { get; }

    // 1. ПРИВАТНЫЙ КОНСТРУКТОР (Никаких throw!)
    private SpeciesBreed(Guid speciesId, Guid breedId)
    {
        SpeciesId = speciesId;
        BreedId = breedId;
    }

    private SpeciesBreed() { } // Для EF Core

    // 2. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
    public static Result<SpeciesBreed, Error> Create(Guid speciesId, Guid breedId)
    {
        if (speciesId == Guid.Empty)
        {
            return Error.Validation("speciesbreed.species_is_empty", "Укажите вид животного (SpeciesId не может быть пустым).");
        }

        if (breedId == Guid.Empty)
        {
            return Error.Validation("speciesbreed.breed_is_empty", "Укажите породу животного (BreedId не может быть пустым).");
        }

        return new SpeciesBreed(speciesId, breedId);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SpeciesId;
        yield return BreedId;
    }
}