namespace PetZone.UseCases.Commands;

public record DeleteBreedCommand(Guid SpeciesId, Guid BreedId);