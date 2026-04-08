using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.BackgroundServices;

public class DailyContentService(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyContentService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GenerateDailyContentAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            logger.LogInformation("DailyContentService: next digest in {Hours:F1} hours", delay.TotalHours);
            await Task.Delay(delay, stoppingToken);

            await GenerateDailyContentAsync(stoppingToken);
        }
    }

    private async Task GenerateDailyContentAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
            var speciesDb = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();

            var today = DateTime.UtcNow.Date;
            var alreadyExists = await db.SystemNewsPosts
                .AnyAsync(p => p.PublishedAt >= today && p.Type == "DailyDigest", cancellationToken);

            if (alreadyExists)
            {
                logger.LogInformation("DailyContentService: today's digest already exists, skipping");
                return;
            }

            var weekAgo = DateTime.UtcNow.AddDays(-7);

            var lookingForHome = await db.Pets
                .CountAsync(p => !p.IsDeleted && p.Status == HelpStatus.LookingForHome, cancellationToken);
            var foundHomeThisWeek = await db.Pets
                .CountAsync(p => !p.IsDeleted && p.Status == HelpStatus.FoundHome && p.CreatedAt >= weekAgo, cancellationToken);
            var needsHelp = await db.Pets
                .CountAsync(p => !p.IsDeleted && p.Status == HelpStatus.NeedsHelp, cancellationToken);
            var totalVolunteers = await db.Volunteers
                .CountAsync(v => !v.IsDeleted, cancellationToken);

            // Top 3 breeds
            var topBreedsJson = await BuildTopBreedsJsonAsync(db, speciesDb, cancellationToken);

            // Top city
            var topCity = await db.Pets
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Location.City)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync(cancellationToken);

            // Featured pet of the day (random pet with photo)
            var petsWithPhotos = await db.Pets
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == HelpStatus.LookingForHome && p.Photos.Any())
                .Select(p => new
                {
                    p.Nickname,
                    p.GeneralDescription,
                    p.Location.City,
                    p.SpeciesBreedInfo.BreedId,
                    Photo = p.Photos.FirstOrDefault(ph => ph.IsMain) ?? p.Photos.First(),
                })
                .ToListAsync(cancellationToken);

            string? featuredNickname = null, featuredPhoto = null,
                    featuredDesc = null, featuredBreed = null, featuredCity = null;

            if (petsWithPhotos.Count > 0)
            {
                var pick = petsWithPhotos[Random.Shared.Next(petsWithPhotos.Count)];
                featuredNickname = pick.Nickname;
                featuredPhoto = pick.Photo.FilePath;
                featuredDesc = pick.GeneralDescription?.Length > 200
                    ? pick.GeneralDescription[..200] + "…"
                    : pick.GeneralDescription;
                featuredCity = pick.City;

                // Look up breed name in Ukrainian
                var breed = await speciesDb.Species
                    .SelectMany(s => s.Breeds)
                    .FirstOrDefaultAsync(b => b.Id == pick.BreedId, cancellationToken);
                featuredBreed = breed?.Translations.GetValueOrDefault("uk")
                             ?? breed?.Translations.GetValueOrDefault("en");
            }

            var factEn = await FetchAnimalFactAsync();

            var post = SystemNewsPost.CreateDigest(
                lookingForHome, needsHelp, foundHomeThisWeek, totalVolunteers, factEn,
                topBreedsJson, topCity,
                featuredNickname, featuredPhoto, featuredDesc, featuredBreed, featuredCity);

            db.SystemNewsPosts.Add(post);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("DailyContentService: digest created for {Date}", today.ToString("dd.MM.yyyy"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DailyContentService: failed to generate daily content");
        }
    }

    private static async Task<string> BuildTopBreedsJsonAsync(
        VolunteersDbContext db, SpeciesDbContext speciesDb, CancellationToken ct)
    {
        var topBreedIds = await db.Pets
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.SpeciesBreedInfo.BreedId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new { BreedId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var ids = topBreedIds.Select(b => b.BreedId).ToList();
        var breeds = await speciesDb.Species
            .SelectMany(s => s.Breeds)
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(ct);

        var result = topBreedIds.Select(b =>
        {
            var breed = breeds.FirstOrDefault(br => br.Id == b.BreedId);
            var name = breed?.Translations.GetValueOrDefault("uk")
                    ?? breed?.Translations.GetValueOrDefault("en")
                    ?? "Невідомо";
            return new { name, count = b.Count };
        }).ToList();

        return JsonSerializer.Serialize(result);
    }

    private static async Task<string> FetchAnimalFactAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            // Alternate between cat and dog facts
            var url = Random.Shared.Next(2) == 0
                ? "https://catfact.ninja/fact"
                : "https://dogapi.dog/api/v2/facts";

            if (url.Contains("dogapi"))
            {
                var dogResp = await http.GetFromJsonAsync<DogFactResponse>(url);
                var fact = dogResp?.Data?.FirstOrDefault()?.Attributes?.Body;
                if (!string.IsNullOrEmpty(fact)) return fact;
            }
            else
            {
                var catResp = await http.GetFromJsonAsync<CatFactResponse>(url);
                if (!string.IsNullOrEmpty(catResp?.Fact)) return catResp.Fact;
            }
        }
        catch { /* fall through */ }

        return DefaultFact();
    }

    private static string DefaultFact() =>
        "Cats sleep about 70% of their lives — that's more than 13 hours a day!";

    private record CatFactResponse([property: JsonPropertyName("fact")] string? Fact);
    private record DogFactResponse([property: JsonPropertyName("data")] List<DogFactItem>? Data);
    private record DogFactItem([property: JsonPropertyName("attributes")] DogFactAttributes? Attributes);
    private record DogFactAttributes([property: JsonPropertyName("body")] string? Body);
}