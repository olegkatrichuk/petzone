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
            logger.LogInformation("Species already seeded, skipping");
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

    private static List<(Dictionary<string, string> Species, List<Dictionary<string, string>> Breeds)> GetSeedData()
    {
        return
        [
            (
                new Dictionary<string, string> { ["uk"] = "Собака", ["en"] = "Dog", ["pl"] = "Pies", ["de"] = "Hund", ["fr"] = "Chien" },
                [
                    new() { ["uk"] = "Лабрадор-ретривер", ["en"] = "Labrador Retriever", ["pl"] = "Labrador Retriever", ["de"] = "Labrador Retriever", ["fr"] = "Labrador Retriever" },
                    new() { ["uk"] = "Хаскі", ["en"] = "Husky", ["pl"] = "Husky", ["de"] = "Husky", ["fr"] = "Husky" },
                    new() { ["uk"] = "Німецька вівчарка", ["en"] = "German Shepherd", ["pl"] = "Owczarek Niemiecki", ["de"] = "Deutscher Schäferhund", ["fr"] = "Berger Allemand" },
                    new() { ["uk"] = "Золотистий ретривер", ["en"] = "Golden Retriever", ["pl"] = "Golden Retriever", ["de"] = "Golden Retriever", ["fr"] = "Golden Retriever" },
                    new() { ["uk"] = "Бульдог", ["en"] = "Bulldog", ["pl"] = "Buldog", ["de"] = "Bulldogge", ["fr"] = "Bouledogue" },
                    new() { ["uk"] = "Пудель", ["en"] = "Poodle", ["pl"] = "Pudel", ["de"] = "Pudel", ["fr"] = "Caniche" },
                    new() { ["uk"] = "Чихуахуа", ["en"] = "Chihuahua", ["pl"] = "Chihuahua", ["de"] = "Chihuahua", ["fr"] = "Chihuahua" },
                    new() { ["uk"] = "Йоркширський тер'єр", ["en"] = "Yorkshire Terrier", ["pl"] = "Yorkshire Terrier", ["de"] = "Yorkshire Terrier", ["fr"] = "Yorkshire Terrier" },
                    new() { ["uk"] = "Шпіц", ["en"] = "Spitz", ["pl"] = "Szpic", ["de"] = "Spitz", ["fr"] = "Spitz" },
                    new() { ["uk"] = "Двородна (безпородна)", ["en"] = "Mixed breed", ["pl"] = "Mieszaniec", ["de"] = "Mischling", ["fr"] = "Bâtard" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Кішка", ["en"] = "Cat", ["pl"] = "Kot", ["de"] = "Katze", ["fr"] = "Chat" },
                [
                    new() { ["uk"] = "Британська короткошерста", ["en"] = "British Shorthair", ["pl"] = "Kot Brytyjski", ["de"] = "Britisch Kurzhaar", ["fr"] = "British Shorthair" },
                    new() { ["uk"] = "Мейн-кун", ["en"] = "Maine Coon", ["pl"] = "Maine Coon", ["de"] = "Maine Coon", ["fr"] = "Maine Coon" },
                    new() { ["uk"] = "Сфінкс", ["en"] = "Sphynx", ["pl"] = "Sfinks", ["de"] = "Sphynx", ["fr"] = "Sphynx" },
                    new() { ["uk"] = "Перська", ["en"] = "Persian", ["pl"] = "Perski", ["de"] = "Perser", ["fr"] = "Persan" },
                    new() { ["uk"] = "Шотландська висловуха", ["en"] = "Scottish Fold", ["pl"] = "Szkocki Zwisłouchy", ["de"] = "Scottish Fold", ["fr"] = "Scottish Fold" },
                    new() { ["uk"] = "Сіамська", ["en"] = "Siamese", ["pl"] = "Syjamski", ["de"] = "Siamkatze", ["fr"] = "Siamois" },
                    new() { ["uk"] = "Бенгальська", ["en"] = "Bengal", ["pl"] = "Bengalski", ["de"] = "Bengalkatze", ["fr"] = "Bengal" },
                    new() { ["uk"] = "Двородна (безпородна)", ["en"] = "Mixed breed", ["pl"] = "Mieszaniec", ["de"] = "Mischling", ["fr"] = "Bâtard" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Кролик", ["en"] = "Rabbit", ["pl"] = "Królik", ["de"] = "Kaninchen", ["fr"] = "Lapin" },
                [
                    new() { ["uk"] = "Вислоухий", ["en"] = "Lop", ["pl"] = "Królik Zwisłouchy", ["de"] = "Widderkaninchen", ["fr"] = "Lapin Bélier" },
                    new() { ["uk"] = "Карликовий", ["en"] = "Dwarf", ["pl"] = "Królik Karłowaty", ["de"] = "Zwergkaninchen", ["fr"] = "Lapin Nain" },
                    new() { ["uk"] = "Ангорський", ["en"] = "Angora", ["pl"] = "Angora", ["de"] = "Angorakaninchen", ["fr"] = "Angora" },
                    new() { ["uk"] = "Рекс", ["en"] = "Rex", ["pl"] = "Rex", ["de"] = "Rex", ["fr"] = "Rex" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Папуга", ["en"] = "Parrot", ["pl"] = "Papuga", ["de"] = "Papagei", ["fr"] = "Perroquet" },
                [
                    new() { ["uk"] = "Хвилястий", ["en"] = "Budgerigar", ["pl"] = "Papużka Falista", ["de"] = "Wellensittich", ["fr"] = "Perruche" },
                    new() { ["uk"] = "Жако", ["en"] = "African Grey", ["pl"] = "Żako", ["de"] = "Graupapagei", ["fr"] = "Gris du Gabon" },
                    new() { ["uk"] = "Корела", ["en"] = "Cockatiel", ["pl"] = "Nimfa", ["de"] = "Nymphensittich", ["fr"] = "Calopsitte" },
                    new() { ["uk"] = "Какаду", ["en"] = "Cockatoo", ["pl"] = "Kakadu", ["de"] = "Kakadu", ["fr"] = "Cacatoès" },
                    new() { ["uk"] = "Ара", ["en"] = "Macaw", ["pl"] = "Ara", ["de"] = "Ara", ["fr"] = "Ara" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Хом'як", ["en"] = "Hamster", ["pl"] = "Chomik", ["de"] = "Hamster", ["fr"] = "Hamster" },
                [
                    new() { ["uk"] = "Джунгарський", ["en"] = "Djungarian", ["pl"] = "Dżungarski", ["de"] = "Dsungarischer Zwerghamster", ["fr"] = "Hamster Dzoungarien" },
                    new() { ["uk"] = "Сирійський", ["en"] = "Syrian", ["pl"] = "Syryjski", ["de"] = "Syrischer Hamster", ["fr"] = "Hamster Syrien" },
                    new() { ["uk"] = "Кампбелла", ["en"] = "Campbell's", ["pl"] = "Campbella", ["de"] = "Campbell-Zwerghamster", ["fr"] = "Hamster de Campbell" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Морська свинка", ["en"] = "Guinea Pig", ["pl"] = "Świnka Morska", ["de"] = "Meerschweinchen", ["fr"] = "Cochon d'Inde" },
                [
                    new() { ["uk"] = "Гладкошерста", ["en"] = "American", ["pl"] = "Amerykańska", ["de"] = "Amerikanisches Meerschweinchen", ["fr"] = "Américain" },
                    new() { ["uk"] = "Перуанська", ["en"] = "Peruvian", ["pl"] = "Peruwijska", ["de"] = "Peruanisches Meerschweinchen", ["fr"] = "Péruvien" },
                    new() { ["uk"] = "Корона", ["en"] = "Coronet", ["pl"] = "Koronka", ["de"] = "Coronet", ["fr"] = "Coronet" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Черепаха", ["en"] = "Turtle", ["pl"] = "Żółw", ["de"] = "Schildkröte", ["fr"] = "Tortue" },
                [
                    new() { ["uk"] = "Червоновуха", ["en"] = "Red-eared Slider", ["pl"] = "Żółw Czerwonolicy", ["de"] = "Rotwangen-Schmuckschildkröte", ["fr"] = "Tortue à Oreilles Rouges" },
                    new() { ["uk"] = "Сухопутна середземноморська", ["en"] = "Mediterranean Tortoise", ["pl"] = "Żółw Lądowy", ["de"] = "Mediterrane Landschildkröte", ["fr"] = "Tortue Méditerranéenne" },
                ]
            ),
            (
                new Dictionary<string, string> { ["uk"] = "Інше", ["en"] = "Other", ["pl"] = "Inne", ["de"] = "Sonstiges", ["fr"] = "Autre" },
                []
            ),
        ];
    }
}
