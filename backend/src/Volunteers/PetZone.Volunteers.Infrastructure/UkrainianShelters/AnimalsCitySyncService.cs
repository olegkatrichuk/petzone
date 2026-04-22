using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.UkrainianShelters;

/// <summary>
/// Syncs animals from animals-city.org (Kharkiv municipal animal shelter).
/// Uses the WordPress REST API: /?rest_route=/wp/v2/posts&amp;categories=17&amp;_embed=1
/// Category 17 = "Прилаштування" (looking for home).
/// </summary>
public class AnimalsCitySyncService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<AnimalsCitySyncService> logger) : BackgroundService
{
    private static readonly Guid SystemVolunteerId = new("cc000000-0000-0000-0000-000000000001");
    private const string BaseUrl   = "https://animals-city.org";
    private const string City      = "Kharkiv";
    private const int    PageSize  = 20;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "animals-city.org sync failed");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting animals-city.org sync");

        using var scope = serviceProvider.CreateScope();
        var db        = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        var speciesDb = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();

        var systemVolunteer = await EnsureSystemVolunteerAsync(db, ct);
        var allSpecies      = await speciesDb.Species.Include(s => s.Breeds).AsNoTracking().ToListAsync(ct);

        // Prefer Dog, fallback to first available
        var defaultSpecies = allSpecies.FirstOrDefault(s =>
            s.Translations.GetValueOrDefault("en", "") == "Dog")
            ?? allSpecies.FirstOrDefault();

        if (defaultSpecies is null)
        {
            logger.LogWarning("No species found in DB, skipping animals-city.org sync");
            return;
        }

        var client   = httpClientFactory.CreateClient("animalsCity");
        var imported = 0;
        var skipped  = 0;

        // Load all existing Kharkiv external IDs upfront — avoids N+1 queries inside the loop
        var existingIds = await db.Pets
            .Where(p => p.ExternalId != null && p.ExternalId.StartsWith("kharkiv:"))
            .Select(p => p.ExternalId!)
            .ToHashSetAsync(ct);

        for (var page = 1; ; page++)
        {
            var url = $"{BaseUrl}/index.php?rest_route=/wp/v2/posts" +
                      $"&categories=17&per_page={PageSize}&page={page}&_embed=1";

            List<AcPost>? posts;
            int totalPages;

            try
            {
                var response = await client.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("animals-city.org API returned {Status} on page {Page}",
                        (int)response.StatusCode, page);
                    break;
                }

                // Total pages from response header
                totalPages = response.Headers.TryGetValues("X-WP-TotalPages", out var hdr)
                    && int.TryParse(hdr.FirstOrDefault(), out var tp) ? tp : page;

                posts = await response.Content.ReadFromJsonAsync<List<AcPost>>(JsonOpts, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch page {Page} from animals-city.org", page);
                break;
            }

            if (posts is null || posts.Count == 0) break;

            foreach (var post in posts)
            {
                try
                {
                    var externalId = $"kharkiv:{post.Id}";

                    if (existingIds.Contains(externalId))
                    {
                        skipped++;
                        continue;
                    }

                    var pet = MapToPet(post, externalId, defaultSpecies, systemVolunteer.Id);
                    if (pet is null) continue;

                    pet.SetExternalId(externalId);
                    pet.SetCountry("ua");
                    if (!string.IsNullOrWhiteSpace(post.Link))
                        pet.SetExternalUrl(post.Link);
                    db.Pets.Add(pet);
                    existingIds.Add(externalId);
                    imported++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to map animals-city.org post {Id}", post.Id);
                }
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("animals-city.org page {Page}/{Total}: imported {Imported}, skipped {Skipped}",
                page, totalPages, imported, skipped);

            if (page >= totalPages) break;
        }

        logger.LogInformation("animals-city.org sync complete: {Imported} imported, {Skipped} skipped",
            imported, skipped);
    }

    private static Pet? MapToPet(
        AcPost post,
        string externalId,
        PetZone.Species.Domain.Species species,
        Guid volunteerId)
    {
        // Strip HTML entities from title
        var name = System.Web.HttpUtility.HtmlDecode(post.Title.Rendered).Trim();
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (name.Length > Pet.MAX_NICKNAME_LENGTH) name = name[..Pet.MAX_NICKNAME_LENGTH];

        // Photo from featured media embed
        var photoUrl = post.Embedded?.FeaturedMedia?.FirstOrDefault()?.SourceUrl;

        // Use mixed breed
        var breed = species.Breeds.FirstOrDefault(b =>
            b.Translations.Values.Any(t => t.Contains("Mix", StringComparison.OrdinalIgnoreCase) ||
                                           t.Contains("Мет", StringComparison.OrdinalIgnoreCase)))
            ?? species.Breeds.FirstOrDefault();

        if (breed is null) return null;

        var speciesBreed = SpeciesBreed.Create(species.Id, breed.Id);
        if (speciesBreed.IsFailure) return null;

        var address = Address.Create(City, "-");
        var weight  = Weight.Create(10.0);
        var height  = Height.Create(40.0);
        var health  = HealthInfo.Create($"Тварина з Харківського міського притулку для тварин.");
        var phone   = PhoneNumber.Create("0000000000");

        if (address.IsFailure || weight.IsFailure || height.IsFailure || health.IsFailure || phone.IsFailure)
            return null;

        var desc = $"Тварина шукає дім. Притулок: animals-city.org (Харків).";

        var pet = Pet.Create(
            id:                  Guid.NewGuid(),
            nickname:            name,
            generalDescription:  desc,
            color:               "Невідомо",
            health:              health.Value,
            location:            address.Value,
            weight:              weight.Value,
            height:              height.Value,
            ownerPhone:          phone.Value,
            isCastrated:         false,
            dateOfBirth:         DateTime.UtcNow.AddYears(-2),
            isVaccinated:        false,
            status:              HelpStatus.LookingForHome,
            microchipNumber:     null,
            volunteerId:         volunteerId,
            adoptionConditions:  null,
            speciesBreedInfo:    speciesBreed.Value);

        if (pet.IsFailure) return null;

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
            FullName.Create("animalsCity", "System").Value,
            Email.Create("system@animals-city.org").Value,
            "System volunteer for animals-city.org (Kharkiv municipal shelter) imported animals.",
            Experience.Create(0).Value,
            PhoneNumber.Create("0000000000").Value).Value;

        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created system volunteer for animals-city.org imports");
        return volunteer;
    }
}