namespace PetZone.Species.Application.Queries;

public record GetBreedsBySpeciesIdQuery(Guid SpeciesId, string Locale = "uk");
