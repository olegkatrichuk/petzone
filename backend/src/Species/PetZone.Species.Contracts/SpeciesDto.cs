namespace PetZone.Species.Contracts;

public record SpeciesDto(Guid Id, string Name, int BreedsCount, Dictionary<string, string> Translations);