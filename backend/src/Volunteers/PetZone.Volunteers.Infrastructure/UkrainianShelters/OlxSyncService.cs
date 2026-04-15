using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.UkrainianShelters;

/// <summary>
/// Syncs free animal adoption ads from OLX.ua (category 1520).
/// API: https://www.olx.ua/api/v1/offers/?category_id=1520&sort_by=created_at:desc
/// </summary>
public class OlxSyncService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<OlxSyncService> logger) : BackgroundService
{
    private static readonly Guid SystemVolunteerId = new("dd000000-0000-0000-0000-000000000001");
    private const string ApiUrl  = "https://www.olx.ua/api/v1/offers/";
    private const int CategoryId = 1520; // free pet adoption on OLX UA
    private const int PageLimit  = 20;
    private const int MaxPages   = 5;   // 100 offers max per sync

    private static readonly Regex HtmlRx  = new(@"<[^>]+>",  RegexOptions.Compiled);
    private static readonly Regex SpaceRx = new(@"\s{2,}",   RegexOptions.Compiled);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(90), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await SyncAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "OLX sync failed"); }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting OLX sync");

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
            logger.LogWarning("No species in DB, skipping OLX sync");
            return;
        }

        var client   = httpClientFactory.CreateClient("olx");
        var imported = 0;
        var skipped  = 0;

        // Load all existing OLX external IDs upfront — avoids N+1 queries inside the loop
        var existingIds = await db.Pets
            .Where(p => p.ExternalId != null && p.ExternalId.StartsWith("olx:"))
            .Select(p => p.ExternalId!)
            .ToHashSetAsync(ct);

        for (var page = 0; page < MaxPages; page++)
        {
            var url = $"{ApiUrl}?offset={page * PageLimit}&limit={PageLimit}" +
                      $"&category_id={CategoryId}&sort_by=created_at:desc";

            var json = await client.GetStringAsync(url, ct);
            using var doc  = JsonDocument.Parse(json);
            var data       = doc.RootElement.GetProperty("data");
            var pageOffers = data.EnumerateArray().ToList();

            foreach (var offer in pageOffers)
            {
                try
                {
                    var id         = offer.GetProperty("id").GetInt64();
                    var externalId = $"olx:{id}";

                    var listingUrl = offer.TryGetProperty("url", out var urlEl)
                        ? urlEl.GetString()
                        : null;

                    if (existingIds.Contains(externalId))
                    {
                        skipped++;
                        continue;
                    }

                    var pet = MapToPet(offer, externalId, listingUrl, catSpecies, dogSpecies, systemVolunteer.Id);
                    if (pet is null) continue;

                    db.Pets.Add(pet);
                    existingIds.Add(externalId);
                    imported++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to map OLX offer");
                }
            }

            await db.SaveChangesAsync(ct);

            if (pageOffers.Count < PageLimit) break; // reached last page
        }

        logger.LogInformation("OLX sync complete: {Imported} imported, {Skipped} skipped", imported, skipped);
    }

    private Pet? MapToPet(
        JsonElement offer,
        string externalId,
        string? listingUrl,
        PetZone.Species.Domain.Species catSpecies,
        PetZone.Species.Domain.Species? dogSpecies,
        Guid volunteerId)
    {
        var title = offer.TryGetProperty("title", out var t) ? t.GetString()?.Trim() ?? "" : "";
        if (string.IsNullOrWhiteSpace(title)) return null;
        if (title.Length > Pet.MAX_NICKNAME_LENGTH) title = title[..Pet.MAX_NICKNAME_LENGTH];

        // Description: strip HTML tags
        var rawDesc = offer.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
        var desc = SpaceRx.Replace(HtmlRx.Replace(rawDesc, " "), " ").Trim();
        if (desc.Length > Pet.MAX_GENERAL_DESCRIPTION_LENGTH)
            desc = desc[..Pet.MAX_GENERAL_DESCRIPTION_LENGTH];
        if (string.IsNullOrWhiteSpace(desc))
            desc = "Тварина шукає дім. Оголошення з OLX.ua.";

        // City
        var city = "Україна";
        if (offer.TryGetProperty("location", out var loc) &&
            loc.TryGetProperty("city", out var cityEl) &&
            cityEl.TryGetProperty("name", out var cityName))
            city = cityName.GetString() ?? "Україна";
        if (city.Length > Address.MAX_CITY_LENGTH) city = city[..Address.MAX_CITY_LENGTH];

        // Params: animal_type
        var animalType = "";
        if (offer.TryGetProperty("params", out var paramsEl))
        {
            foreach (var param in paramsEl.EnumerateArray())
            {
                if (!param.TryGetProperty("key", out var k)) continue;
                if (k.GetString() == "animal_type" &&
                    param.TryGetProperty("value", out var v) &&
                    v.TryGetProperty("label", out var lbl))
                {
                    animalType = lbl.GetString() ?? "";
                    break;
                }
            }
        }

        // Species: dog or cat (match on animalType label from OLX params)
        var isDog = animalType.Contains("Собак", StringComparison.OrdinalIgnoreCase)
                 || animalType.Contains("Цуценят", StringComparison.OrdinalIgnoreCase)
                 || animalType.Contains("собак", StringComparison.OrdinalIgnoreCase);

        // Fall back to cat only when we know it's not a dog (or dog species doesn't exist)
        var species = isDog && dogSpecies is not null && dogSpecies.Id != catSpecies.Id
            ? dogSpecies
            : catSpecies;

        // Health hints from description
        var searchText   = (title + " " + desc).ToLowerInvariant();
        var isCastrated  = searchText.Contains("кастрован") || searchText.Contains("стерилізован") || searchText.Contains("стерилизован");
        var isVaccinated = searchText.Contains("привит") || searchText.Contains("щеплен") || searchText.Contains("вакцинован");

        // Photo: replace size template
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

        // Use mixed breed
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
        var health  = HealthInfo.Create("Тварина з оголошення OLX.ua.");
        var phone   = PhoneNumber.Create("0000000000");

        if (address.IsFailure || wt.IsFailure || ht.IsFailure || health.IsFailure || phone.IsFailure)
            return null;

        var pet = Pet.Create(
            id:                 Guid.NewGuid(),
            nickname:           title,
            generalDescription: desc,
            color:              "Невідомо",
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
            FullName.Create("olx",    "System").Value,
            Email.Create("system@olx.petzone").Value,
            "System volunteer for OLX.ua imported animals.",
            Experience.Create(0).Value,
            PhoneNumber.Create("0000000000").Value).Value;

        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created system volunteer for OLX imports");
        return volunteer;
    }
}