namespace PetZone.Species.Contracts;

public record BreedDto(Guid Id, string Name, Dictionary<string, string> Translations);