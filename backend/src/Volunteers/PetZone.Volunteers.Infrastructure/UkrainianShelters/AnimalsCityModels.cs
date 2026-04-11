using System.Text.Json.Serialization;

namespace PetZone.Volunteers.Infrastructure.UkrainianShelters;

public record AcPost(
    [property: JsonPropertyName("id")]             int Id,
    [property: JsonPropertyName("title")]          AcTitle Title,
    [property: JsonPropertyName("link")]           string? Link,
    [property: JsonPropertyName("featured_media")] int FeaturedMedia,
    [property: JsonPropertyName("_embedded")]      AcEmbedded? Embedded);

public record AcTitle(
    [property: JsonPropertyName("rendered")] string Rendered);

public record AcEmbedded(
    [property: JsonPropertyName("wp:featuredmedia")] List<AcMedia>? FeaturedMedia);

public record AcMedia(
    [property: JsonPropertyName("source_url")] string? SourceUrl);