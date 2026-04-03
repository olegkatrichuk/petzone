using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpeciesEntity = PetZone.Species.Domain.Species;
using BreedEntity = PetZone.Species.Domain.Breed;

namespace PetZone.Species.Infrastructure;

public static class SpeciesSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SpeciesDbContext>>();

        if (await dbContext.Species.AnyAsync())
        {
            logger.LogInformation("Species already seeded, checking for missing translations...");
            await AddMissingTranslationsAsync(dbContext, logger);
            return;
        }

        var data = GetSeedData();

        foreach (var (speciesTranslations, breeds) in data)
        {
            var speciesResult = SpeciesEntity.Create(Guid.NewGuid(), speciesTranslations);
            if (speciesResult.IsFailure)
            {
                logger.LogWarning("Failed to create species: {Error}", speciesResult.Error.Description);
                continue;
            }

            var species = speciesResult.Value;

            foreach (var breedTranslations in breeds)
            {
                var breedResult = BreedEntity.Create(Guid.NewGuid(), breedTranslations);
                if (breedResult.IsFailure)
                {
                    logger.LogWarning("Failed to create breed: {Error}", breedResult.Error.Description);
                    continue;
                }

                species.AddBreed(breedResult.Value);
            }

            dbContext.Species.Add(species);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Species and breeds seeded successfully");
    }

    private static async Task AddMissingTranslationsAsync(SpeciesDbContext dbContext, ILogger logger)
    {
        var ruTranslations = GetRuTranslations();
        var allSpecies = await dbContext.Species.Include(s => s.Breeds).ToListAsync();
        var changed = false;

        foreach (var species in allSpecies)
        {
            if (species.Translations.ContainsKey("ru")) continue;

            // Match by uk name
            var ukName = species.Translations.GetValueOrDefault("uk", "");
            if (ruTranslations.Species.TryGetValue(ukName, out var ruName))
            {
                species.Translations["ru"] = ruName;
                changed = true;

                foreach (var breed in species.Breeds)
                {
                    if (breed.Translations.ContainsKey("ru")) continue;
                    var ukBreedName = breed.Translations.GetValueOrDefault("uk", "");
                    if (ruTranslations.Breeds.TryGetValue(ukBreedName, out var ruBreedName))
                        breed.Translations["ru"] = ruBreedName;
                }
            }
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Added missing Russian translations to species/breeds");
        }
    }

    private static (Dictionary<string, string> Species, Dictionary<string, string> Breeds) GetRuTranslations() => (
        new Dictionary<string, string>
        {
            ["Собака"] = "Собака",
            ["Кішка"] = "Кошка",
            ["Кролик"] = "Кролик",
            ["Папуга"] = "Попугай",
            ["Хом'як"] = "Хомяк",
            ["Морська свинка"] = "Морская свинка",
            ["Черепаха"] = "Черепаха",
            ["Інше"] = "Другое",
        },
        new Dictionary<string, string>
        {
            ["Лабрадор-ретривер"] = "Лабрадор-ретривер",
            ["Хаскі"] = "Хаски",
            ["Німецька вівчарка"] = "Немецкая овчарка",
            ["Золотистий ретривер"] = "Золотистый ретривер",
            ["Бульдог"] = "Бульдог",
            ["Пудель"] = "Пудель",
            ["Чихуахуа"] = "Чихуахуа",
            ["Йоркширський тер'єр"] = "Йоркширский терьер",
            ["Шпіц"] = "Шпиц",
            ["Двородна (безпородна)"] = "Метис (беспородная)",
            ["Британська короткошерста"] = "Британская короткошёрстная",
            ["Мейн-кун"] = "Мейн-кун",
            ["Сфінкс"] = "Сфинкс",
            ["Перська"] = "Персидская",
            ["Шотландська висловуха"] = "Шотландская вислоухая",
            ["Сіамська"] = "Сиамская",
            ["Бенгальська"] = "Бенгальская",
            ["Вислоухий"] = "Вислоухий",
            ["Карликовий"] = "Карликовый",
            ["Ангорський"] = "Ангорский",
            ["Рекс"] = "Рекс",
            ["Хвилястий"] = "Волнистый попугайчик",
            ["Жако"] = "Жако",
            ["Корела"] = "Корелла",
            ["Какаду"] = "Какаду",
            ["Ара"] = "Ара",
            ["Джунгарський"] = "Джунгарский",
            ["Сирійський"] = "Сирийский",
            ["Кампбелла"] = "Кэмпбелла",
            ["Гладкошерста"] = "Гладкошёрстная",
            ["Перуанська"] = "Перуанская",
            ["Корона"] = "Коронет",
            ["Червоновуха"] = "Красноухая черепаха",
            ["Сухопутна середземноморська"] = "Средиземноморская черепаха",
        }
    );

    private static List<(Dictionary<string, string> Species, List<Dictionary<string, string>> Breeds)> GetSeedData()
    {
        return
        [
            (
                new Dictionary<string, string> { ["uk"] = "Собака", ["ru"] = "Собака", ["en"] = "Dog", ["pl"] = "Pies", ["de"] = "Hund", ["fr"] = "Chien" },
                [
                    new() { ["uk"] = "Лабрадор-ретривер", ["ru"] = "Лабрадор-ретривер", ["en"] = "Labrador Retriever", ["pl"] = "Labrador Retriever", ["de"] = "Labrador Retriever", ["fr"] = "Labrador Retriever" },
                    new() { ["uk"] = "Хаскі", ["ru"] = "Хаски", ["en"] = "Husky", ["pl"] = "Husky", ["de"] = "Husky", ["fr"] = "Husky" },
                    new() { ["uk"] = "Німецька вівчарка", ["ru"] = "Немецкая овчарка", ["en"] = "German Shepherd", ["pl"] = "Owczarek Niemiecki", ["de"] = "Deutscher Schäferhund", ["fr"] = "Berger Allemand" },
                    new() { ["uk"] = "Золотистий ретривер", ["ru"] = "Золотистый ретривер", ["en"] = "Golden Retriever", ["pl"] = "Golden Retriever", ["de"] = "Golden Retriever", ["fr"] = "Golden Retriever" },
                    new() { ["uk"] = "Бульдог", ["ru"] = "Бульдог", ["en"] = "Bulldog", ["pl"] = "Buldog", ["de"] = "Bulldogge", ["fr"] = "Bouledogue" },
                    new() { ["uk"] = "Пудель", ["ru"] = "Пудель", ["en"] = "Poodle", ["pl"] = "Pudel", ["de"] = "Pudel", ["fr"] = "Caniche" },
                    new() { ["uk"] = "Чихуахуа", ["ru"] = "Чихуахуа", ["en"] = "Chihuahua", ["pl"] = "Chihuahua", ["de"] = "Chihuahua", ["fr"] = "Chihuahua" },
                    new() { ["uk"] = "Йоркширський тер'єр", ["ru"] = "Йоркширский терьер", ["en"] = "Yorkshire Terrier", ["pl"] = "Yorkshire Terrier", ["de"] = "Yorkshire Terrier", ["fr"] = "Yorkshire Terrier" },
                    new() { ["uk"] = "Шпіц", ["ru"] = "Шпиц", ["en"] = "Spitz", ["pl"] = "Szpic", ["de"] = "Spitz", ["fr"] = "Spitz" },
                    new() { ["uk"] = "Двородна (безпородна)", ["ru"] = "Метис (беспородная)", ["en"] = "Mixed breed", ["pl"] = "Mieszaniec", ["de"] = "Mischling", ["fr"] = "Bâtard" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Кішка", ["ru"] = "Кошка", ["en"] = "Cat", ["pl"] = "Kot", ["de"] = "Katze", ["fr"] = "Chat" },
                [
                    new() { ["uk"] = "Британська короткошерста", ["ru"] = "Британская короткошёрстная", ["en"] = "British Shorthair", ["pl"] = "Kot Brytyjski", ["de"] = "Britisch Kurzhaar", ["fr"] = "British Shorthair" },
                    new() { ["uk"] = "Мейн-кун", ["ru"] = "Мейн-кун", ["en"] = "Maine Coon", ["pl"] = "Maine Coon", ["de"] = "Maine Coon", ["fr"] = "Maine Coon" },
                    new() { ["uk"] = "Сфінкс", ["ru"] = "Сфинкс", ["en"] = "Sphynx", ["pl"] = "Sfinks", ["de"] = "Sphynx", ["fr"] = "Sphynx" },
                    new() { ["uk"] = "Перська", ["ru"] = "Персидская", ["en"] = "Persian", ["pl"] = "Perski", ["de"] = "Perser", ["fr"] = "Persan" },
                    new() { ["uk"] = "Шотландська висловуха", ["ru"] = "Шотландская вислоухая", ["en"] = "Scottish Fold", ["pl"] = "Szkocki Zwisłouchy", ["de"] = "Scottish Fold", ["fr"] = "Scottish Fold" },
                    new() { ["uk"] = "Сіамська", ["ru"] = "Сиамская", ["en"] = "Siamese", ["pl"] = "Syjamski", ["de"] = "Siamkatze", ["fr"] = "Siamois" },
                    new() { ["uk"] = "Бенгальська", ["ru"] = "Бенгальская", ["en"] = "Bengal", ["pl"] = "Bengalski", ["de"] = "Bengalkatze", ["fr"] = "Bengal" },
                    new() { ["uk"] = "Двородна (безпородна)", ["ru"] = "Метис (беспородная)", ["en"] = "Mixed breed", ["pl"] = "Mieszaniec", ["de"] = "Mischling", ["fr"] = "Bâtard" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Кролик", ["ru"] = "Кролик", ["en"] = "Rabbit", ["pl"] = "Królik", ["de"] = "Kaninchen", ["fr"] = "Lapin" },
                [
                    new() { ["uk"] = "Вислоухий", ["ru"] = "Вислоухий", ["en"] = "Lop", ["pl"] = "Królik Zwisłouchy", ["de"] = "Widderkaninchen", ["fr"] = "Lapin Bélier" },
                    new() { ["uk"] = "Карликовий", ["ru"] = "Карликовый", ["en"] = "Dwarf", ["pl"] = "Królik Karłowaty", ["de"] = "Zwergkaninchen", ["fr"] = "Lapin Nain" },
                    new() { ["uk"] = "Ангорський", ["ru"] = "Ангорский", ["en"] = "Angora", ["pl"] = "Angora", ["de"] = "Angorakaninchen", ["fr"] = "Angora" },
                    new() { ["uk"] = "Рекс", ["ru"] = "Рекс", ["en"] = "Rex", ["pl"] = "Rex", ["de"] = "Rex", ["fr"] = "Rex" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Папуга", ["ru"] = "Попугай", ["en"] = "Parrot", ["pl"] = "Papuga", ["de"] = "Papagei", ["fr"] = "Perroquet" },
                [
                    new() { ["uk"] = "Хвилястий", ["ru"] = "Волнистый попугайчик", ["en"] = "Budgerigar", ["pl"] = "Papużka Falista", ["de"] = "Wellensittich", ["fr"] = "Perruche" },
                    new() { ["uk"] = "Жако", ["ru"] = "Жако", ["en"] = "African Grey", ["pl"] = "Żako", ["de"] = "Graupapagei", ["fr"] = "Gris du Gabon" },
                    new() { ["uk"] = "Корела", ["ru"] = "Корелла", ["en"] = "Cockatiel", ["pl"] = "Nimfa", ["de"] = "Nymphensittich", ["fr"] = "Calopsitte" },
                    new() { ["uk"] = "Какаду", ["ru"] = "Какаду", ["en"] = "Cockatoo", ["pl"] = "Kakadu", ["de"] = "Kakadu", ["fr"] = "Cacatoès" },
                    new() { ["uk"] = "Ара", ["ru"] = "Ара", ["en"] = "Macaw", ["pl"] = "Ara", ["de"] = "Ara", ["fr"] = "Ara" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Хом'як", ["ru"] = "Хомяк", ["en"] = "Hamster", ["pl"] = "Chomik", ["de"] = "Hamster", ["fr"] = "Hamster" },
                [
                    new() { ["uk"] = "Джунгарський", ["ru"] = "Джунгарский", ["en"] = "Djungarian", ["pl"] = "Dżungarski", ["de"] = "Dsungarischer Zwerghamster", ["fr"] = "Hamster Dzoungarien" },
                    new() { ["uk"] = "Сирійський", ["ru"] = "Сирийский", ["en"] = "Syrian", ["pl"] = "Syryjski", ["de"] = "Syrischer Hamster", ["fr"] = "Hamster Syrien" },
                    new() { ["uk"] = "Кампбелла", ["ru"] = "Кэмпбелла", ["en"] = "Campbell's", ["pl"] = "Campbella", ["de"] = "Campbell-Zwerghamster", ["fr"] = "Hamster de Campbell" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Морська свинка", ["ru"] = "Морская свинка", ["en"] = "Guinea Pig", ["pl"] = "Świnka Morska", ["de"] = "Meerschweinchen", ["fr"] = "Cochon d'Inde" },
                [
                    new() { ["uk"] = "Гладкошерста", ["ru"] = "Гладкошёрстная", ["en"] = "American", ["pl"] = "Amerykańska", ["de"] = "Amerikanisches Meerschweinchen", ["fr"] = "Américain" },
                    new() { ["uk"] = "Перуанська", ["ru"] = "Перуанская", ["en"] = "Peruvian", ["pl"] = "Peruwijska", ["de"] = "Peruanisches Meerschweinchen", ["fr"] = "Péruvien" },
                    new() { ["uk"] = "Корона", ["ru"] = "Коронет", ["en"] = "Coronet", ["pl"] = "Koronka", ["de"] = "Coronet", ["fr"] = "Coronet" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Черепаха", ["ru"] = "Черепаха", ["en"] = "Turtle", ["pl"] = "Żółw", ["de"] = "Schildkröte", ["fr"] = "Tortue" },
                [
                    new() { ["uk"] = "Червоновуха", ["ru"] = "Красноухая черепаха", ["en"] = "Red-eared Slider", ["pl"] = "Żółw Czerwonolicy", ["de"] = "Rotwangen-Schmuckschildkröte", ["fr"] = "Tortue à Oreilles Rouges" },
                    new() { ["uk"] = "Сухопутна середземноморська", ["ru"] = "Средиземноморская черепаха", ["en"] = "Mediterranean Tortoise", ["pl"] = "Żółw Lądowy", ["de"] = "Mediterrane Landschildkröte", ["fr"] = "Tortue Méditerranéenne" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Інше", ["ru"] = "Другое", ["en"] = "Other", ["pl"] = "Inne", ["de"] = "Sonstiges", ["fr"] = "Autre" },
                []
            ),
        ];
    }
}
