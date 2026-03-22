namespace PetZone.Species.Application.Commands;

public record DeleteBreedCommand(Guid SpeciesId, Guid BreedId);