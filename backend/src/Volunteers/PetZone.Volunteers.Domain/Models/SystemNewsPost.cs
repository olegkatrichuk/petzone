namespace PetZone.Volunteers.Domain.Models;

public class SystemNewsPost
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public DateTime PublishedAt { get; private set; }

    // Structured stats — rendered and translated on the frontend
    public int LookingForHome { get; private set; }
    public int NeedsHelp { get; private set; }
    public int FoundHomeThisWeek { get; private set; }
    public int TotalVolunteers { get; private set; }

    // Fact stored in English; frontend translates via MyMemory API
    public string FactEn { get; private set; } = string.Empty;

    // Top breeds JSON: [{name, count}, ...]
    public string TopBreedsJson { get; private set; } = "[]";

    // City with most pets
    public string? TopCity { get; private set; }

    // Featured pet of the day
    public string? FeaturedPetNickname { get; private set; }
    public string? FeaturedPetPhotoUrl { get; private set; }
    public string? FeaturedPetDescription { get; private set; }
    public string? FeaturedPetBreed { get; private set; }
    public string? FeaturedPetCity { get; private set; }

    private SystemNewsPost() { }

    public static SystemNewsPost CreateDigest(
        int lookingForHome, int needsHelp, int foundHomeThisWeek,
        int totalVolunteers, string factEn,
        string topBreedsJson, string? topCity,
        string? featuredPetNickname, string? featuredPetPhotoUrl,
        string? featuredPetDescription, string? featuredPetBreed, string? featuredPetCity) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = "DailyDigest",
            PublishedAt = DateTime.UtcNow,
            LookingForHome = lookingForHome,
            NeedsHelp = needsHelp,
            FoundHomeThisWeek = foundHomeThisWeek,
            TotalVolunteers = totalVolunteers,
            FactEn = factEn.Trim(),
            TopBreedsJson = topBreedsJson,
            TopCity = topCity,
            FeaturedPetNickname = featuredPetNickname,
            FeaturedPetPhotoUrl = featuredPetPhotoUrl,
            FeaturedPetDescription = featuredPetDescription,
            FeaturedPetBreed = featuredPetBreed,
            FeaturedPetCity = featuredPetCity,
        };
}
