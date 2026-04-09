using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.UkrainianShelters;

/// <summary>
/// Syncs animals from lkplev.com (Lviv municipal veterinary sterilisation centre).
/// Listing page: https://lkplev.com/adoption/
/// </summary>
public class LkplevSyncService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<LkplevSyncService> logger) : BackgroundService
{
    private static readonly Guid SystemVolunteerId = new("bb000000-0000-0000-0000-000000000001");
    private const string ListingUrl = "https://lkplev.com/adoption/";
    private const string BaseUrl    = "https://lkplev.com";
    private const string City       = "Lviv";

    // Regex patterns (DOTALL so . crosses newlines)
    private static readonly Regex AnimalBlockRx = new(
        @"<div class=""animal"">(.*?)</div>\s*</div>\s*</div>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex NameRx        = new(@"<h3[^>]*>([^<]+)</h3>",       RegexOptions.Compiled);
    private static readonly Regex IdRx          = new(@"href=""/detail/view/(\d+)""",  RegexOptions.Compiled);
    private static readonly Regex PhotoRx       = new(@"<img\s+src=""([^""]+)""",      RegexOptions.Compiled);
    private static readonly Regex GenderRx      = new(@"Стать:.*?>\s*([А-Яа-яЄєІіЇїҐґ']+)\s*<", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex BirthdateRx   = new(@"data-birthdate=""([^""]+)""",  RegexOptions.Compiled);
    private static readonly Regex SizeRx        = new(@"Розмір:.*?>\s*([А-Яа-яЄєІіЇїҐґ']+)\s*<", RegexOptions.Singleline | RegexOptions.Compiled);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "lkplev.com sync failed");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting lkplev.com sync");

        using var scope = serviceProvider.CreateScope();
        var db        = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        var speciesDb = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();

        var systemVolunteer = await EnsureSystemVolunteerAsync(db, ct);
        var allSpecies      = await speciesDb.Species.Include(s => s.Breeds).AsNoTracking().ToListAsync(ct);

        // Prefer Cat, fallback to first available
        var catSpecies = allSpecies.FirstOrDefault(s =>
            s.Translations.GetValueOrDefault("en", "") == "Cat")
            ?? allSpecies.FirstOrDefault();

        if (catSpecies is null)
        {
            logger.LogWarning("No species found in DB, skipping lkplev.com sync");
            return;
        }

        var client  = httpClientFactory.CreateClient("lkplev");
        var html    = await client.GetStringAsync(ListingUrl, ct);
        var blocks  = AnimalBlockRx.Matches(html);

        var imported = 0;
        var skipped  = 0;

        foreach (Match block in blocks)
        {
            var content = block.Value;
            try
            {
                var idMatch = IdRx.Match(content);
                if (!idMatch.Success) continue;

                var externalId = $"lkplev:{idMatch.Groups[1].Value}";

                if (await db.Pets.AnyAsync(p => p.ExternalId == externalId, ct))
                {
                    skipped++;
                    continue;
                }

                var pet = MapToPet(content, externalId, catSpecies, systemVolunteer.Id);
                if (pet is null)
                    continue;

                pet.SetExternalId(externalId);
                db.Pets.Add(pet);
                imported++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to map lkplev animal block");
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("lkplev.com sync complete: {Imported} imported, {Skipped} skipped",
            imported, skipped);
    }

    private Pet? MapToPet(
        string block,
        string externalId,
        PetZone.Species.Domain.Species catSpecies,
        Guid volunteerId)
    {
        var name = NameRx.Match(block).Groups[1].Value.Trim();
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (name.Length > Pet.MAX_NICKNAME_LENGTH) name = name[..Pet.MAX_NICKNAME_LENGTH];

        // Photo URL (first img in block, skipping anchor target)
        var photoUrl = PhotoRx.Match(block).Groups[1].Value.Trim();

        // Gender: Дівчинка / Самиця → female, Хлопчик / Самець → male
        var genderText  = GenderRx.Match(block).Groups[1].Value.Trim().ToLowerInvariant();
        var isCastrated = genderText is "дівчинка" or "самиця"; // sterilisation centre — assume sterilised

        // Birthdate
        DateTime dob;
        var bdMatch = BirthdateRx.Match(block);
        dob = bdMatch.Success && DateTime.TryParse(bdMatch.Groups[1].Value, out var parsed)
            ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
            : DateTime.UtcNow.AddYears(-2);

        // Size → weight / height estimate
        var sizeText = SizeRx.Match(block).Groups[1].Value.Trim().ToLowerInvariant();
        var (weight, height) = sizeText switch
        {
            "маленький" or "маленька" => (2.5, 20.0),
            "середній"  or "середня"  => (5.0, 30.0),
            "великий"   or "велика"   => (10.0, 50.0),
            _                         => (4.0, 25.0),
        };

        // Use mixed breed
        var breed = catSpecies.Breeds.FirstOrDefault(b =>
            b.Translations.Values.Any(t => t.Contains("Mix", StringComparison.OrdinalIgnoreCase) ||
                                           t.Contains("Мет", StringComparison.OrdinalIgnoreCase)))
            ?? catSpecies.Breeds.FirstOrDefault();

        if (breed is null) return null;

        var speciesBreed = SpeciesBreed.Create(catSpecies.Id, breed.Id);
        if (speciesBreed.IsFailure) return null;

        var address  = Address.Create(City, "-");
        var wt       = Weight.Create(weight);
        var ht       = Height.Create(height);
        var health   = HealthInfo.Create($"Тварина з притулку ЛКП Лев, м. Львів. Стерилізована.");
        var phone    = PhoneNumber.Create("0000000000");

        if (address.IsFailure || wt.IsFailure || ht.IsFailure || health.IsFailure || phone.IsFailure)
            return null;

        var desc = $"Тварина шукає дім. Притулок: ЛКП Лев (Львів).";

        var pet = Pet.Create(
            id:                  Guid.NewGuid(),
            nickname:            name,
            generalDescription:  desc,
            color:               "Невідомо",
            health:              health.Value,
            location:            address.Value,
            weight:              wt.Value,
            height:              ht.Value,
            ownerPhone:          phone.Value,
            isCastrated:         isCastrated,
            dateOfBirth:         dob,
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
            FullName.Create("lkplev", "System").Value,
            Email.Create("system@lkplev.com").Value,
            "System volunteer for lkplev.com (Lviv municipal shelter) imported animals.",
            Experience.Create(0).Value,
            PhoneNumber.Create("0000000000").Value).Value;

        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created system volunteer for lkplev.com imports");
        return volunteer;
    }
}