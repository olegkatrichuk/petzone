using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.RescueGroups;

public class RescueGroupsSyncService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    IOptions<RescueGroupsOptions> options,
    ILogger<RescueGroupsSyncService> logger) : BackgroundService
{
    private static readonly Guid SystemVolunteerId = new("aa000000-0000-0000-0000-000000000001");
    private readonly RescueGroupsOptions _opts = options.Value;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Species name mappings (RescueGroups singular → PetZone EN name)
    private static readonly Dictionary<string, string> SpeciesMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dog"] = "Dog",
        ["cat"] = "Cat",
        ["rabbit"] = "Rabbit",
        ["bird"] = "Parrot",
        ["guinea pig"] = "Guinea Pig",
        ["hamster"] = "Hamster",
        ["turtle"] = "Turtle",
    };

    // sizeCurrent from RescueGroups is weight in pounds
    private static double[] SizeCurrentToWeightHeight(double? sizeLbs)
    {
        var kg = (sizeLbs ?? 10) * 0.453592;
        kg = Math.Max(0.5, kg);
        var height = kg switch { < 5 => 20.0, < 15 => 40.0, < 30 => 60.0, _ => 80.0 };
        return [Math.Round(kg, 1), height];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay to let the app fully start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RescueGroups sync failed");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opts.ApiKey))
        {
            logger.LogWarning("RescueGroups:ApiKey is not configured, skipping sync");
            return;
        }

        logger.LogInformation("Starting RescueGroups sync");

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        var speciesDb = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();

        var systemVolunteer = await EnsureSystemVolunteerAsync(db, ct);
        var allSpecies = await speciesDb.Species.Include(s => s.Breeds).AsNoTracking().ToListAsync(ct);

        var client = httpClientFactory.CreateClient("RescueGroups");

        var imported = 0;
        var skipped = 0;

        for (var page = 1; page <= _opts.MaxPages; page++)
        {
            var url = $"animals/search/available?limit={_opts.PageSize}&page={page}" +
                      "&include=species,locations";

            RgApiResponse? response;
            try
            {
                var httpResponse = await client.GetAsync(url, ct);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var body = await httpResponse.Content.ReadAsStringAsync(ct);
                    logger.LogError("RescueGroups API returned {Status} for page {Page}: {Body}",
                        (int)httpResponse.StatusCode, page, body[..Math.Min(500, body.Length)]);
                    break;
                }
                response = await httpResponse.Content.ReadFromJsonAsync<RgApiResponse>(JsonOpts, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch page {Page} from RescueGroups", page);
                break;
            }

            if (response?.Data == null || response.Data.Count == 0)
                break;

            var includedMap = (response.Included ?? [])
                .ToDictionary(i => (i.Type, i.Id));

            foreach (var animal in response.Data)
            {
                try
                {
                    var externalId = $"rg:{animal.Id}";

                    if (await db.Pets.AnyAsync(p => p.ExternalId == externalId, ct))
                    {
                        skipped++;
                        continue;
                    }

                    var pet = MapToPet(animal, includedMap, allSpecies, systemVolunteer.Id);
                    if (pet is null)
                        continue;

                    pet.SetExternalId(externalId);
                    db.Pets.Add(pet);
                    imported++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to map animal {Id}", animal.Id);
                }
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation("Page {Page}/{Pages}: imported {Imported}, skipped {Skipped}",
                page, response.Meta?.Pages ?? page, imported, skipped);

            if (page >= (response.Meta?.Pages ?? page))
                break;
        }

        logger.LogInformation("RescueGroups sync complete: {Imported} imported, {Skipped} skipped",
            imported, skipped);
    }

    private static Pet? MapToPet(
        RgAnimal animal,
        Dictionary<(string Type, string Id), RgIncluded> includedMap,
        List<PetZone.Species.Domain.Species> allSpecies,
        Guid volunteerId)
    {
        var attrs = animal.Attributes;
        if (string.IsNullOrWhiteSpace(attrs.Name))
            return null;

        // Resolve species
        var speciesName = "Other";
        if (animal.Relationships?.Species?.Data is { } speciesRef &&
            includedMap.TryGetValue(("species", speciesRef.Id), out var speciesIncluded) &&
            speciesIncluded.Attributes?.Singular is { } singular)
        {
            speciesName = SpeciesMap.GetValueOrDefault(singular, "Other");
        }

        var species = allSpecies.FirstOrDefault(s =>
            s.Translations.GetValueOrDefault("en", "") == speciesName)
            ?? allSpecies.FirstOrDefault(s =>
            s.Translations.GetValueOrDefault("en", "") == "Other");

        if (species is null) return null;

        // Match breed by name or fall back to "Mixed breed"
        var breedName = attrs.BreedPrimary?.Trim() ?? "";
        var breed = species.Breeds.FirstOrDefault(b =>
            b.Translations.Values.Any(t =>
                t.Contains(breedName, StringComparison.OrdinalIgnoreCase) ||
                breedName.Contains(t, StringComparison.OrdinalIgnoreCase)))
            ?? species.Breeds.FirstOrDefault(b =>
            b.Translations.GetValueOrDefault("en", "").Contains("Mixed", StringComparison.OrdinalIgnoreCase))
            ?? species.Breeds.FirstOrDefault();

        if (breed is null) return null;

        var speciesBreed = SpeciesBreed.Create(species.Id, breed.Id);
        if (speciesBreed.IsFailure) return null;

        // Location
        var city = "Unknown";
        if (animal.Relationships?.Locations?.Data is { } locRef &&
            includedMap.TryGetValue(("locations", locRef.Id), out var locIncluded))
        {
            city = locIncluded.Attributes?.City ?? city;
        }
        city = city.Length > Address.MAX_CITY_LENGTH ? city[..Address.MAX_CITY_LENGTH] : city;
        var address = Address.Create(city, "-");
        if (address.IsFailure) return null;

        // Size → weight/height (sizeCurrent is weight in lbs)
        var dims = SizeCurrentToWeightHeight(attrs.SizeCurrent);
        var weight = Weight.Create(dims[0]);
        var height = Height.Create(dims[1]);
        if (weight.IsFailure || height.IsFailure) return null;

        // Health
        var healthDesc = attrs.DescriptionText?.Trim() ?? "No health information available.";
        if (healthDesc.Length > HealthInfo.MAX_GENERAL_DESCRIPTION_LENGTH)
            healthDesc = healthDesc[..HealthInfo.MAX_GENERAL_DESCRIPTION_LENGTH];
        var health = HealthInfo.Create(healthDesc.Length > 0 ? healthDesc : "Healthy.");
        if (health.IsFailure) return null;

        // Date of birth
        var dob = attrs.BirthDate ?? EstimateDob(attrs.AgeGroup);

        // Description
        var desc = attrs.DescriptionText?.Trim() ?? $"Meet {attrs.Name}!";
        if (string.IsNullOrWhiteSpace(desc)) desc = $"Meet {attrs.Name}!";
        if (desc.Length > Pet.MAX_GENERAL_DESCRIPTION_LENGTH)
            desc = desc[..Pet.MAX_GENERAL_DESCRIPTION_LENGTH];

        // Color
        var color = attrs.Color?.Trim() ?? "Unknown";
        if (string.IsNullOrWhiteSpace(color)) color = "Unknown";
        if (color.Length > Pet.MAX_COLOR_LENGTH) color = color[..Pet.MAX_COLOR_LENGTH];

        var phone = PhoneNumber.Create("0000000000");
        if (phone.IsFailure) return null;

        var pet = Pet.Create(
            id: Guid.NewGuid(),
            nickname: attrs.Name.Length > Pet.MAX_NICKNAME_LENGTH ? attrs.Name[..Pet.MAX_NICKNAME_LENGTH] : attrs.Name,
            generalDescription: desc,
            color: color,
            health: health.Value,
            location: address.Value,
            weight: weight.Value,
            height: height.Value,
            ownerPhone: phone.Value,
            isCastrated: attrs.IsNeutered ?? false,
            dateOfBirth: dob,
            isVaccinated: attrs.IsVaccinated ?? false,
            status: HelpStatus.LookingForHome,
            microchipNumber: null,
            volunteerId: volunteerId,
            adoptionConditions: null,
            speciesBreedInfo: speciesBreed.Value);

        if (pet.IsFailure) return null;

        // Photo
        if (!string.IsNullOrWhiteSpace(attrs.PictureThumbnailUrl))
        {
            var photoResult = PetPhoto.Create(attrs.PictureThumbnailUrl, true);
            if (photoResult.IsSuccess)
                pet.Value.AddPhoto(photoResult.Value);
        }

        return pet.Value;
    }

    private static DateTime EstimateDob(string? ageGroup) => ageGroup?.ToLower() switch
    {
        "baby"   => DateTime.UtcNow.AddMonths(-2),
        "young"  => DateTime.UtcNow.AddYears(-1),
        "adult"  => DateTime.UtcNow.AddYears(-3),
        "senior" => DateTime.UtcNow.AddYears(-7),
        _        => DateTime.UtcNow.AddYears(-2),
    };

    private async Task<Volunteer> EnsureSystemVolunteerAsync(VolunteersDbContext db, CancellationToken ct)
    {
        var existing = await db.Volunteers
            .FirstOrDefaultAsync(v => v.Id == SystemVolunteerId, ct);

        if (existing is not null)
            return existing;

        var name = FullName.Create("RescueGroups", "System").Value;
        var email = Email.Create("system@rescuegroups.org").Value;
        var exp = Experience.Create(0).Value;
        var phone = PhoneNumber.Create("0000000000").Value;

        var volunteerResult = Volunteer.Create(
            SystemVolunteerId, Guid.Empty, name, email,
            "System volunteer for RescueGroups.org imported animals.",
            exp, phone);

        var volunteer = volunteerResult.Value;
        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created system volunteer for RescueGroups imports");
        return volunteer;
    }
}
