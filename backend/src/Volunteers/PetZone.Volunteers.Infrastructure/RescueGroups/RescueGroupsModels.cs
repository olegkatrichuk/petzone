using System.Text.Json.Serialization;

namespace PetZone.Volunteers.Infrastructure.RescueGroups;

public record RgApiResponse(
    [property: JsonPropertyName("meta")] RgMeta? Meta,
    [property: JsonPropertyName("data")] List<RgAnimal> Data,
    [property: JsonPropertyName("included")] List<RgIncluded>? Included);

public record RgMeta(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("countReturned")] int CountReturned,
    [property: JsonPropertyName("pageReturned")] int PageReturned,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("pages")] int Pages);

public record RgAnimal(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("attributes")] RgAnimalAttributes Attributes,
    [property: JsonPropertyName("relationships")] RgAnimalRelationships? Relationships);

public record RgAnimalAttributes(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("sex")] string? Sex,
    [property: JsonPropertyName("ageGroup")] string? AgeGroup,
    [property: JsonPropertyName("birthDate")] DateTime? BirthDate,
    [property: JsonPropertyName("breedPrimary")] string? BreedPrimary,
    [property: JsonPropertyName("color")] string? Color,
    [property: JsonPropertyName("descriptionText")] string? DescriptionText,
    [property: JsonPropertyName("pictureThumbnailUrl")] string? PictureThumbnailUrl,
    [property: JsonPropertyName("sizeCurrent")] double? SizeCurrent,
    [property: JsonPropertyName("isNeutered")] bool? IsNeutered,
    [property: JsonPropertyName("isMicrochipped")] bool? IsMicrochipped,
    [property: JsonPropertyName("isVaccinated")] bool? IsVaccinated);

public record RgAnimalRelationships(
    [property: JsonPropertyName("species")] RgRelationship? Species,
    [property: JsonPropertyName("locations")] RgRelationship? Locations);

public record RgRelationship(
    [property: JsonPropertyName("data")] RgRelationshipData? Data);

public record RgRelationshipData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type);

public record RgIncluded(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("attributes")] RgIncludedAttributes? Attributes);

public record RgIncludedAttributes(
    // species
    [property: JsonPropertyName("singular")] string? Singular,
    // location
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State);