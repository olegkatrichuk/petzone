namespace PetZone.Species.Application.Commands;

public record CreateBreedCommand(Guid SpeciesId, Dictionary<string, string> Translations);