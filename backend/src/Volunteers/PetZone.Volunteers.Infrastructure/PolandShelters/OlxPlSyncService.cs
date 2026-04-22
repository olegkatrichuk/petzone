using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.PolandShelters;

/// <summary>
/// Syncs free animal adoption ads from OLX.pl (categories 90/91 — dogs/cats).
/// API: https://www.olx.pl/api/v1/offers/?category_id=90&sort_by=created_at:desc
/// </summary>
public class OlxPlSyncService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<OlxPlSyncService> logger) : BackgroundService
{
    private static readonly Guid SystemVolunteerId = new("ee000000-0000-0000-0000-000000000001");
    private const string ApiUrl   = "https://www.olx.pl/api/v1/offers/";
    private const int DogCategory = 90;  // psy / dogs on OLX.pl
    private const int CatCategory = 91;  // koty / cats on OLX.pl
    private const int PageLimit   = 20;
    private const int MaxPages    = 5;   // 100 offers per category per sync

    private static readonly Regex HtmlRx  = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex SpaceRx = new(@"\s{2,}",  RegexOptions.Compiled);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Stagger startup — OLX UA starts at 90 s, PL starts at 120 s
        await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await SyncAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "OLX PL sync failed"); }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting OLX PL sync");

        using var scope     = serviceProvider.CreateScope();
        var db              = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        var speciesDb       = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();
        var systemVolunteer = await EnsureSystemVolunteerAsync(db, ct);

        var allSpecies = await speciesDb.Species.Include(s => s.Breeds).AsNoTracking().ToListAsync(ct);
        var catSpecies = allSpecies.FirstOrDefault(s => s.Translations.GetValueOrDefault("en", "") == "Cat")
                         ?? allSpecies.FirstOrDefault();
        var dogSpecies = allSpecies.FirstOrDefault(s => s.Translations.GetValueOrDefault("en", "") == "Dog")
                         ?? catSpecies;

        if (catSpecies is null)
        {
            logger.LogWarning("No species in DB, skipping OLX PL sync");
            return;
        }

        var client   = httpClientFactory.CreateClient("olx-pl");
        var imported = 0;
        var skipped  = 0;

        var existingIds = await db.Pets
            .Where(p => p.ExternalId != null && p.ExternalId.StartsWith("pl:"))
            .Select(p => p.ExternalId!)
            .ToHashSetAsync(ct);

        foreach (var (categoryId, species) in new[] {
            (DogCategory, dogSpecies ?? catSpecies),
            (CatCategory, catSpecies) })
        {
            for (var page = 0; page < MaxPages; page++)
            {
                var url = $"{ApiUrl}?offset={page * PageLimit}&limit={PageLimit}" +
                          $"&category_id={categoryId}&sort_by=created_at:desc";

                var json = await client.GetStringAsync(url, ct);
                using var doc  = JsonDocument.Parse(json);
                var data       = doc.RootElement.GetProperty("data");
                var pageOffers = data.EnumerateArray().ToList();

                foreach (var offer in pageOffers)
                {
                    try
                    {
                        var id         = offer.GetProperty("id").GetInt64();
                        var externalId = $"pl:{id}";

                        var listingUrl = offer.TryGetProperty("url", out var urlEl)
                            ? urlEl.GetString()
                            : null;

                        if (existingIds.Contains(externalId))
                        {
                            skipped++;
                            continue;
                        }

                        var pet = MapToPet(offer, externalId, listingUrl, species, systemVolunteer.Id);
                        if (pet is null) continue;

                        db.Pets.Add(pet);
                        existingIds.Add(externalId);
                        imported++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to map OLX PL offer");
                    }
                }

                await db.SaveChangesAsync(ct);

                if (pageOffers.Count < PageLimit) break;
            }
        }

        logger.LogInformation("OLX PL sync complete: {Imported} imported, {Skipped} skipped", imported, skipped);
    }

    private Pet? MapToPet(
        JsonElement offer,
        string externalId,
        string? listingUrl,
        PetZone.Species.Domain.Species species,
        Guid volunteerId)
    {
        var title = offer.TryGetProperty("title", out var t) ? t.GetString()?.Trim() ?? "" : "";
        if (string.IsNullOrWhiteSpace(title)) return null;
        if (title.Length > Pet.MAX_NICKNAME_LENGTH) title = title[..Pet.MAX_NICKNAME_LENGTH];

        var rawDesc = offer.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
        var desc    = SpaceRx.Replace(HtmlRx.Replace(rawDesc, " "), " ").Trim();
        if (desc.Length > Pet.MAX_GENERAL_DESCRIPTION_LENGTH)
            desc = desc[..Pet.MAX_GENERAL_DESCRIPTION_LENGTH];
        if (string.IsNullOrWhiteSpace(desc))
            desc = "Zwierzę szuka domu. Ogłoszenie z OLX.pl.";

        var city = "Polska";
        if (offer.TryGetProperty("location", out var loc) &&
            loc.TryGetProperty("city", out var cityEl) &&
            cityEl.TryGetProperty("name", out var cityName))
            city = cityName.GetString() ?? "Polska";
        if (city.Length > Address.MAX_CITY_LENGTH) city = city[..Address.MAX_CITY_LENGTH];

        var searchText   = (title + " " + desc).ToLowerInvariant();
        var isCastrated  = searchText.Contains("kastrowan") || searchText.Contains("sterylizow");
        var isVaccinated = searchText.Contains("szczepion") || searchText.Contains("zaszczepion");

        var isDog = externalId.StartsWith("pl:") &&
                    (searchText.Contains("pies") || searchText.Contains("psa") || searchText.Contains("szczeni"));

        var photoUrl = "";
        if (offer.TryGetProperty("photos", out var photos))
        {
            foreach (var photo in photos.EnumerateArray())
            {
                if (photo.TryGetProperty("link", out var link))
                {
                    photoUrl = (link.GetString() ?? "").Replace("{width}x{height}", "600x450");
                    break;
                }
            }
        }

        var breed = species.Breeds.FirstOrDefault(b =>
            b.Translations.Values.Any(v =>
                v.Contains("Mix", StringComparison.OrdinalIgnoreCase) ||
                v.Contains("Мет", StringComparison.OrdinalIgnoreCase)))
                    ?? species.Breeds.FirstOrDefault();
        if (breed is null) return null;

        var speciesBreed = SpeciesBreed.Create(species.Id, breed.Id);
        if (speciesBreed.IsFailure) return null;

        var address = Address.Create(city, "-");
        var wt      = Weight.Create(isDog ? 8.0 : 4.0);
        var ht      = Height.Create(isDog ? 35.0 : 25.0);
        var health  = HealthInfo.Create("Zwierzę z ogłoszenia OLX.pl.");
        var phone   = PhoneNumber.Create("0000000000");

        if (address.IsFailure || wt.IsFailure || ht.IsFailure || health.IsFailure || phone.IsFailure)
            return null;

        var pet = Pet.Create(
            id:                 Guid.NewGuid(),
            nickname:           title,
            generalDescription: desc,
            color:              "Nieznany",
            health:             health.Value,
            location:           address.Value,
            weight:             wt.Value,
            height:             ht.Value,
            ownerPhone:         phone.Value,
            isCastrated:        isCastrated,
            dateOfBirth:        DateTime.UtcNow.AddYears(-2),
            isVaccinated:       isVaccinated,
            status:             HelpStatus.LookingForHome,
            microchipNumber:    null,
            volunteerId:        volunteerId,
            adoptionConditions: null,
            speciesBreedInfo:   speciesBreed.Value);

        if (pet.IsFailure) return null;

        pet.Value.SetExternalId(externalId);
        pet.Value.SetCountry("pl");

        if (!string.IsNullOrWhiteSpace(listingUrl))
            pet.Value.SetExternalUrl(listingUrl);

        if (!string.IsNullOrWhiteSpace(photoUrl))
        {
            var photo = PetPhoto.Create(photoUrl, true);
            if (photo.IsSuccess) pet.Value.AddPhoto(photo.Value);
        }

        return pet.Value;
    }

    private async Task<Volunteer> EnsureSystemVolunteerAsync(VolunteersDbContext db, CancellationToken ct)
    {
        var existing = await db.Volunteers.FirstOrDefaultAsync(v => v.Id == SystemVolunteerId, ct);
        if (existing is not null) return existing;

        var volunteer = Volunteer.Create(
            SystemVolunteerId,
            Guid.Empty,
            FullName.Create("olx-pl", "System").Value,
            Email.Create("system@olx-pl.petzone").Value,
            "System volunteer for OLX.pl imported animals.",
            Experience.Create(0).Value,
            PhoneNumber.Create("0000000000").Value).Value;

        volunteer.MarkAsSystem();
        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created system volunteer for OLX PL imports");
        return volunteer;
    }
}
