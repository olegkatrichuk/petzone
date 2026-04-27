using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure;

public static class VolunteersSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        var speciesDb = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<VolunteersDbContext>>();

        // Allow disabling the seeder via config — set "Seeding:Enabled": false in production
        // to prevent accidental re-seeding on every deploy.
        var seedingEnabled = config.GetValue<bool?>("Seeding:Enabled") ?? true;
        if (!seedingEnabled)
        {
            logger.LogInformation("Seeding is disabled (Seeding:Enabled=false). Skipping.");
            return;
        }

        var allSpecies = await speciesDb.Species.Include(s => s.Breeds).ToListAsync();
        var dogSpecies = allSpecies.FirstOrDefault(s => s.Translations.GetValueOrDefault("en") == "Dog");
        var catSpecies = allSpecies.FirstOrDefault(s => s.Translations.GetValueOrDefault("en") == "Cat");

        if (dogSpecies is null || catSpecies is null)
        {
            logger.LogWarning("Species not seeded yet, skipping volunteers seeding.");
            return;
        }

        var volunteerDefs = TryLoadFromJson(logger) ?? GetVolunteerDefs();

        // One bulk query for all existing emails — avoids N individual round-trips
        // and eliminates the EF Core complex-property translation risk on per-row AnyAsync.
        var allDefinedEmails = volunteerDefs.Select(v => v.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingEmails = (await db.Volunteers
            .Select(v => v.Email.Value)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Collect ExternalId prefixes already present in DB to skip imported country data.
        var existingPrefixes = await db.Volunteers
            .SelectMany(v => v.Pets)
            .Where(p => p.ExternalId != null)
            .Select(p => p.ExternalId!)
            .Distinct()
            .ToListAsync();

        // Unsplash fallback photos (used only for hardcoded defs without photo_url)
        var unsplashKey = config["Unsplash:AccessKey"] ?? "";
        var dogPhotos = new Queue<string>(await FetchPhotos("dog puppy", 30, unsplashKey, logger));
        var catPhotos = new Queue<string>(await FetchPhotos("cat kitten", 30, unsplashKey, logger));

        var savedCount = 0;

        foreach (var vd in volunteerDefs)
        {
            // Skip volunteer if all its pets have ExternalId prefixes already in DB
            var petPrefixes = vd.Pets
                .Where(p => !string.IsNullOrEmpty(p.ExternalId))
                .Select(p => p.ExternalId!.Split(':')[0] + ":")
                .Distinct()
                .ToList();

            if (petPrefixes.Any() && petPrefixes.All(prefix =>
                existingPrefixes.Any(e => e.StartsWith(prefix))))
            {
                logger.LogDebug("Skipping volunteer — country prefix {Prefix} already seeded", string.Join(",", petPrefixes));
                continue;
            }

            // Fast in-memory email check against the pre-loaded set
            if (existingEmails.Contains(vd.Email))
                continue;

            var name = FullName.Create(vd.FirstName, vd.LastName, vd.Patronymic);
            var email = Email.Create(vd.Email);
            var exp = Experience.Create(vd.Years);
            var phone = PhoneNumber.Create(vd.Phone);

            if (name.IsFailure || email.IsFailure || exp.IsFailure || phone.IsFailure) continue;

            var volunteerResult = Volunteer.Create(Guid.NewGuid(), Guid.Empty, name.Value, email.Value, vd.Description, exp.Value, phone.Value);
            if (volunteerResult.IsFailure) continue;

            var volunteer = volunteerResult.Value;

            foreach (var pd in vd.Pets)
            {
                var speciesEntity = pd.IsDog ? dogSpecies : catSpecies;
                var breed = speciesEntity.Breeds.FirstOrDefault(b => b.Translations.GetValueOrDefault("en") == pd.BreedEn)
                    ?? speciesEntity.Breeds.FirstOrDefault(b => b.Translations.GetValueOrDefault("en") == "Mixed breed")
                    ?? speciesEntity.Breeds.First();

                // Use photo from JSON if available, otherwise Unsplash queue
                string photoUrl;
                if (!string.IsNullOrEmpty(pd.PhotoUrl))
                {
                    photoUrl = pd.PhotoUrl;
                }
                else
                {
                    var queue = pd.IsDog ? dogPhotos : catPhotos;
                    photoUrl = queue.Count > 0 ? queue.Dequeue() : FallbackPhoto(pd.IsDog);
                }

                var sb = SpeciesBreed.Create(speciesEntity.Id, breed.Id);
                var health = HealthInfo.Create(pd.HealthDesc);
                var address = Address.Create(pd.City, string.IsNullOrWhiteSpace(pd.Street) ? "-" : pd.Street);
                var weight = Weight.Create(pd.Weight);
                var height = Height.Create(pd.Height);
                var ownerPhone = PhoneNumber.Create(vd.Phone);

                if (sb.IsFailure || health.IsFailure || address.IsFailure || weight.IsFailure || height.IsFailure || ownerPhone.IsFailure)
                    continue;

                var petResult = Pet.Create(
                    Guid.NewGuid(), pd.Nickname, pd.Description, pd.Color,
                    health.Value, address.Value, weight.Value, height.Value,
                    ownerPhone.Value, pd.IsCastrated,
                    DateTime.SpecifyKind(pd.DateOfBirth, DateTimeKind.Utc),
                    pd.IsVaccinated, pd.Status, null, volunteer.Id, pd.AdoptionConditions, sb.Value);

                if (petResult.IsFailure) continue;
                var pet = petResult.Value;

                if (!string.IsNullOrEmpty(pd.ExternalId))
                    pet.SetExternalId(pd.ExternalId);

                var photoResult = PetPhoto.Create(photoUrl, true);
                if (photoResult.IsSuccess)
                    pet.AddPhoto(photoResult.Value);

                volunteer.AddPet(pet);
            }

            try
            {
                db.Volunteers.Add(volunteer);
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
                savedCount++;
            }
            catch (Exception ex)
            {
                db.ChangeTracker.Clear();
                logger.LogError(ex, "Failed to seed volunteer {FirstName} {LastName}: {Error}",
                    vd.FirstName, vd.LastName, ex.InnerException?.Message ?? ex.Message);
            }
        }

        logger.LogInformation("Volunteers seeded: {Count}/{Total}", savedCount, volunteerDefs.Count);
    }

    private static List<VolunteerDef>? TryLoadFromJson(ILogger logger)
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "seed_data.json");
        if (!File.Exists(jsonPath))
            return null;

        try
        {
            var json = File.ReadAllText(jsonPath);
            var root = JsonSerializer.Deserialize<JsonSeedRoot>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            if (root?.Volunteers is not { Count: > 0 } vols)
                return null;

            var defs = vols
                .Where(v => v.Pets.Count > 0)
                .Select(v => new VolunteerDef(
                    v.FirstName, v.LastName, v.Patronymic ?? "Іванівна",
                    v.Email, v.Phone, v.Years, v.Description,
                    v.Pets.Select(p => new PetDef(
                        p.IsDog, p.BreedEn ?? "Mixed breed",
                        p.Nickname, p.Color ?? "різнокольоровий",
                        p.Weight, p.Height, p.City, p.Street,
                        p.HealthDesc ?? "Оглянутий ветеринаром.",
                        p.Description, p.AdoptionConditions,
                        p.IsCastrated, p.IsVaccinated,
                        DateTime.TryParse(p.DateOfBirth, out var dob) ? dob : DateTime.UtcNow.AddYears(-2),
                        p.Status switch
                        {
                            "FoundHome" => HelpStatus.FoundHome,
                            "NeedsHelp" => HelpStatus.NeedsHelp,
                            _ => HelpStatus.LookingForHome,
                        },
                        p.PhotoUrl, p.ExternalId))
                    .ToList()))
                .ToList();

            logger.LogInformation("Loading {Count} volunteers from seed_data.json", defs.Count);
            return defs;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load seed_data.json, using hardcoded data");
            return null;
        }
    }

    private record JsonSeedRoot(List<JsonVolunteer>? Volunteers);

    private record JsonVolunteer(
        string FirstName, string LastName, string? Patronymic,
        string Email, string Phone, int Years, string Description,
        List<JsonPet> Pets);

    private record JsonPet(
        bool IsDog, string? BreedEn, string Nickname, string? Color,
        double Weight, double Height, string City, string? Street,
        string? HealthDesc, string Description, string? AdoptionConditions,
        bool IsCastrated, bool IsVaccinated, string DateOfBirth,
        string Status = "LookingForHome", string? PhotoUrl = null,
        string? ExternalId = null);

    // ── Unsplash ──────────────────────────────────────────────────────────────

    private static async Task<List<string>> FetchPhotos(string query, int count, string accessKey, ILogger logger)
    {
        if (string.IsNullOrEmpty(accessKey)) return [];
        try
        {
            using var http = new HttpClient();
            var url = $"https://api.unsplash.com/search/photos?query={Uri.EscapeDataString(query)}&per_page={Math.Min(count, 30)}&client_id={accessKey}";
            var result = await http.GetFromJsonAsync<UnsplashSearchResponse>(url);
            return result?.Results?
                .Select(r => r.Urls.Raw.Split('?')[0] + "?w=800&h=600&q=80&auto=format&fit=crop")
                .ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch Unsplash photos for '{Query}'", query);
            return [];
        }
    }

    private static string FallbackPhoto(bool isDog) => isDog
        ? "https://images.unsplash.com/photo-1552053831-71594a27632d?w=800&h=600&q=80&auto=format&fit=crop"
        : "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=800&h=600&q=80&auto=format&fit=crop";

    private record UnsplashSearchResponse([property: JsonPropertyName("results")] List<UnsplashPhoto>? Results);
    private record UnsplashPhoto([property: JsonPropertyName("urls")] UnsplashUrls Urls);
    private record UnsplashUrls([property: JsonPropertyName("raw")] string Raw);

    // ── Seed data ─────────────────────────────────────────────────────────────

    private static List<VolunteerDef> GetVolunteerDefs() =>
    [
        new(
            "Олена", "Коваленко", "Сергіївна", "olena.kovalenko@petzone.ua", "+380671234501", 5,
            "Люблю тварин із самого дитинства. Допомагаю безпритульним собакам та кішкам знайти теплий дім. За 5 років разом з командою влаштували понад 80 вихованців.",
            [
                new(true, "Лабрадор Retriever", "Барон", "чорний", 32, 62, "Київ", "вул. Хрещатик, 15",
                    "Повністю здоровий, пройшов всі щеплення. Енергійний, любить дітей.",
                    "Любить активні прогулянки та ігри з м'ячем. Відмінно знає команди.",
                    "Сім'я з досвідом утримання великих собак, є власний двір.",
                    true, true, new DateTime(2021, 3, 10), HelpStatus.LookingForHome),

                new(true, "Husky", "Сніжок", "біло-сірий", 24, 56, "Київ", "вул. Саксаганського, 7",
                    "Здоровий, всі щеплення зроблені. Хворів на отит — вилікуваний.",
                    "Дуже активний і голосистий. Обожнює сніг і довгі пробіжки. Потребує простору.",
                    "Досвідчений власник, бажано заміський будинок або велика квартира з вигулом 2+ рази на день.",
                    true, true, new DateTime(2020, 11, 5), HelpStatus.LookingForHome),

                new(false, "British Shorthair", "Пуся", "блакитний", 4.2, 28, "Київ", "вул. Льва Толстого, 3",
                    "Абсолютно здорова, стерилізована, щеплена. Живе в квартирі.",
                    "Спокійна, любить ласку, але не нав'язлива. Ладнає з дітьми та іншими котами.",
                    "Сім'я або самотній власник. Квартира чи будинок.",
                    true, true, new DateTime(2019, 7, 22), HelpStatus.LookingForHome),

                new(false, "Scottish Fold", "Масік", "рудий", 3.8, 25, "Київ", "вул. Антоновича, 44",
                    "Здоровий, кастрований, всі щеплення актуальні.",
                    "Обожнює лежати на колінах і дивитись телевізор. Дуже лагідний.",
                    null, true, true, new DateTime(2020, 4, 15), HelpStatus.LookingForHome),

                new(true, "German Shepherd", "Мухтар", "чорно-підпалий", 36, 64, "Київ", "вул. Велика Васильківська, 120",
                    "Здоровий, всі щеплення є. Проходив курс дресирування.",
                    "Розумний і відданий. Знає 15+ команд. Підходить для охорони та активних сімей.",
                    "Досвідчений власник, бажано приватний будинок.",
                    true, true, new DateTime(2020, 8, 30), HelpStatus.NeedsHelp),
            ]),

        new(
            "Дмитро", "Шевченко", "Олегович", "dmytro.shevchenko@petzone.ua", "+380672345602", 3,
            "Волонтерю в харківському притулку вже три роки. Спеціалізуюся на реабілітації тварин після травм. Допоміг знайти домівку більш ніж 50 вихованцям.",
            [
                new(true, "Golden Retriever", "Арчі", "золотистий", 28, 58, "Харків", "вул. Сумська, 67",
                    "Повністю здоровий, щеплений. Стерилізація не потрібна — кобель.",
                    "Дружелюбний, добрий, обожнює дітей. Ідеальний сімейний пес.",
                    "Буде радий будь-якій люблячій сім'ї.",
                    false, true, new DateTime(2022, 2, 14), HelpStatus.LookingForHome),

                new(true, "Mixed breed", "Дружок", "коричнево-білий", 14, 42, "Харків", "вул. Пушкінська, 18",
                    "Після лікування перелому повністю відновився. Щеплений, кастрований.",
                    "Знайшли на вулиці після аварії. Зараз весела і вдячна собака. Любить всіх без винятку.",
                    null, true, true, new DateTime(2019, 5, 3), HelpStatus.LookingForHome),

                new(false, "Maine Coon", "Рудик", "рудий табі", 6.1, 35, "Харків", "пр. Науки, 14",
                    "Здоровий, кастрований. Всі щеплення актуальні.",
                    "Великий, м'який і спокійний. Любить спати на людях. Не кряхтить.",
                    "Квартира чи будинок. Не потребує вигулу.",
                    true, true, new DateTime(2018, 9, 17), HelpStatus.LookingForHome),

                new(true, "Bulldog", "Боксер", "палевий", 22, 40, "Харків", "вул. Клочківська, 200",
                    "Здоровий, перевірений ветеринаром. Є незначний дерматит — контрольований дієтою.",
                    "Флегматичний, любить поспати, не потребує великих навантажень. Обожнює лежати у ніг господаря.",
                    "Квартира підходить, головне — не залишати надовго одного.",
                    true, true, new DateTime(2021, 6, 8), HelpStatus.LookingForHome),

                new(false, "Siamese", "Кіса", "кремовий з темними мітками", 3.4, 24, "Харків", "вул. Гоголя, 5",
                    "Здорова, стерилізована, щеплена.",
                    "Балакуча і прив'язана до людей. Буде вірним другом самотній людині.",
                    null, true, true, new DateTime(2021, 1, 25), HelpStatus.LookingForHome),
            ]),

        new(
            "Марія", "Бойко", "Василівна", "mariia.boyko@petzone.ua", "+380673456703", 7,
            "Засновниця волонтерської групи «Лапки Львова». Сім років рятуємо тварин з вулиці, лікуємо та знаходимо нові домівки. Понад 200 щасливих фіналів.",
            [
                new(true, "Yorkshire Terrier", "Йорк", "рудо-сталевий", 2.8, 20, "Львів", "вул. Шевченка, 12",
                    "Здоровий, щеплений, кастрований.",
                    "Маленький, але хоробрий. Підходить для квартири. Не гавкає без причини.",
                    "Будь-яка люблячча родина.",
                    true, true, new DateTime(2022, 8, 10), HelpStatus.LookingForHome),

                new(false, "Persian", "Сніжана", "білий", 4.5, 26, "Львів", "пл. Ринок, 8",
                    "Здорова, стерилізована. Перської потребує регулярного грумінгу.",
                    "Царственна і ніжна. Любить тихий дім без гучних звуків.",
                    "Спокійна квартира, хазяїн з часом для догляду за шерстю.",
                    true, true, new DateTime(2020, 12, 3), HelpStatus.LookingForHome),

                new(true, "Labrador Retriever", "Белла", "шоколадна", 29, 60, "Львів", "вул. Городоцька, 77",
                    "Після операції на кульшовому суглобі повністю одужала. Щеплена.",
                    "Тиха, лагідна лабрадорка. Ідеальна для сімей з дітьми та людей похилого віку.",
                    "Квартира або будинок. Помірні прогулянки 2 рази на день.",
                    true, true, new DateTime(2019, 3, 20), HelpStatus.LookingForHome),

                new(false, "Bengal", "Тигр", "коричневий мармур", 5.0, 32, "Львів", "вул. Франка, 23",
                    "Здоровий, кастрований, щеплений.",
                    "Дуже активний, розумний. Потребує ігрових комплексів та уваги. Харчування — тільки натуральне.",
                    "Досвідчений власник котів, простора квартира.",
                    true, true, new DateTime(2021, 7, 11), HelpStatus.LookingForHome),

                new(true, "Poodle", "Кучер", "білий", 6.5, 35, "Львів", "вул. Личаківська, 45",
                    "Здоровий, щеплений, стрижений. Гіпоалергенна порода.",
                    "Інтелігентний, навчений, грайливий. Не залишає вовни на меблях.",
                    null, true, true, new DateTime(2020, 10, 5), HelpStatus.LookingForHome),

                new(false, "Sphynx", "Гомер", "персиковий", 3.9, 30, "Львів", "вул. Дорошенка, 11",
                    "Здоровий, кастрований, щеплений. Потребує регулярного купання.",
                    "Ніжний і теплий у прямому розумінні слова. Стане найкращим грілкою взимку.",
                    "Квартира, без протягів, з теплими місцями для сну.",
                    true, true, new DateTime(2022, 3, 7), HelpStatus.LookingForHome),
            ]),

        new(
            "Андрій", "Мельник", "Іванович", "andrii.melnyk@petzone.ua", "+380674567804", 2,
            "Волонтерю в одеському приморському районі. Допомагаю кошенятам і цуценятам, яких знаходять біля моря. Є власна тимчасова сім'я для 4 підопічних.",
            [
                new(false, "Mixed breed", "Сонька", "тигровий", 2.1, 20, "Одеса", "вул. Дерибасівська, 33",
                    "Здорова, стерилізована, щеплена. Знайдена кошеням біля порту.",
                    "Граційна і незалежна. Любить спостерігати за птахами з вікна.",
                    null, true, true, new DateTime(2023, 6, 1), HelpStatus.LookingForHome),

                new(true, "Chihuahua", "Тузик", "кремовий", 2.5, 18, "Одеса", "пров. Чайковського, 5",
                    "Здоровий, щеплений. Дуже мало їсть, зручний для маленьких квартир.",
                    "Сміливий, незважаючи на малий розмір. Відданий одному господарю.",
                    "Спокійна обстановка, без маленьких дітей.",
                    true, true, new DateTime(2022, 4, 19), HelpStatus.LookingForHome),

                new(false, "British Shorthair", "Лорд", "сірий", 5.3, 29, "Одеса", "вул. Катерининська, 16",
                    "Здоровий, кастрований, щеплений.",
                    "Величний, плюшевий і спокійний. Не руйнує меблі. Ідеальний кіт для зайнятих людей.",
                    null, true, true, new DateTime(2020, 2, 8), HelpStatus.LookingForHome),

                new(true, "Spitz", "Лаки", "оранжевий", 3.2, 28, "Одеса", "вул. Рішельєвська, 27",
                    "Здоровий, щеплений. Невелика алергія на деякі корми — контрольована.",
                    "Пухнастий і жвавий. Підходить для квартири, головне — щоденний вигул.",
                    "Активна сім'я або одинак з часом для прогулянок.",
                    false, true, new DateTime(2021, 9, 14), HelpStatus.LookingForHome),

                new(false, "Maine Coon", "Гарфілд", "помаранчевий табі", 7.2, 38, "Одеса", "вул. Пушкінська, 44",
                    "Здоровий, кастрований, щеплений. Найбільший кіт у нашому притулку.",
                    "М'який велетень. Обожнює гратись з водою і переслідувати іграшки.",
                    "Простора квартира чи будинок.",
                    true, true, new DateTime(2019, 11, 30), HelpStatus.LookingForHome),
            ]),

        new(
            "Наталія", "Гриценко", "Михайлівна", "nataliia.hrytsenko@petzone.ua", "+380675678905", 4,
            "Очолюю групу волонтерів у Дніпрі. Допомагаємо тваринам, постраждалим під час бомбардувань. Наша команда з 8 людей рятує і знаходить нові домівки для підопічних по всій Україні.",
            [
                new(true, "Mixed breed", "Рекс", "чорний", 20, 50, "Дніпро", "просп. Гагаріна, 72",
                    "Після контузії від вибуху — повністю реабілітований. Щеплений, кастрований.",
                    "Пережив багато, але залишився добрим і довірливим. Шукає спокійну люблячу сім'ю.",
                    "Спокійна домівка без гучних звуків перший час.",
                    true, true, new DateTime(2018, 6, 15), HelpStatus.LookingForHome),

                new(false, "Scottish Fold", "Зірка", "сірий мармур", 3.6, 24, "Дніпро", "вул. Робоча, 15",
                    "Здорова, стерилізована, щеплена. Евакуйована зі східних районів.",
                    "Швидко звикає до нового місця. Ніжна і тиха.",
                    null, true, true, new DateTime(2021, 5, 20), HelpStatus.LookingForHome),

                new(true, "German Shepherd", "Ріка", "чорно-рудий", 27, 60, "Дніпро", "вул. Набережна Перемоги, 3",
                    "Здорова, всі щеплення є. Пройшла базовий курс дресирування.",
                    "Активна і розумна вівчарка. Добре взаємодіє з дітьми. Потребує простору і руху.",
                    "Будинок або велика квартира з регулярним вигулом.",
                    false, true, new DateTime(2022, 1, 8), HelpStatus.LookingForHome),

                new(false, "Mixed breed", "Муся", "трьохколірна", 3.0, 22, "Дніпро", "вул. Гончарова, 56",
                    "Здорова, стерилізована, щеплена. Знайдена в покинутій квартирі.",
                    "Поступово відкривається людям. Мирна і спокійна. Не чіпляється і не кряхтить.",
                    null, true, true, new DateTime(2022, 9, 3), HelpStatus.LookingForHome),

                new(true, "Labrador Retriever", "Буян", "жовтий", 31, 62, "Дніпро", "вул. Сєрова, 34",
                    "Здоровий, щеплений. Невелика жировик видалений — без наслідків.",
                    "Веселий та енергійний. Обожнює воду і плавання. Ідеальний для активних господарів.",
                    "Активна сім'я, наявність водойми поруч — перевага.",
                    false, true, new DateTime(2020, 7, 25), HelpStatus.LookingForHome),
            ]),
    ];

    // ── Record types ──────────────────────────────────────────────────────────

    private record VolunteerDef(
        string FirstName, string LastName, string Patronymic,
        string Email, string Phone, int Years, string Description,
        List<PetDef> Pets);

    private record PetDef(
        bool IsDog, string BreedEn, string Nickname, string Color,
        double Weight, double Height, string City, string? Street,
        string HealthDesc, string Description, string? AdoptionConditions,
        bool IsCastrated, bool IsVaccinated, DateTime DateOfBirth, HelpStatus Status,
        string? PhotoUrl = null, string? ExternalId = null);
}