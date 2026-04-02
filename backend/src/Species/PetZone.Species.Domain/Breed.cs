using PetZone.SharedKernel;

namespace PetZone.Species.Domain
{
    public class Breed : Entity<Guid>
    {
        public const int MAX_NAME_LENGTH = 100;

        public Dictionary<string, string> Translations { get; private set; } = new();

        private Breed(Guid id, Dictionary<string, string> translations) : base(id)
        {
            Translations = translations;
        }

        private Breed() { }

        public string GetName(string locale)
        {
            if (Translations.TryGetValue(locale, out var name)) return name;
            if (Translations.TryGetValue("uk", out name)) return name;
            if (Translations.TryGetValue("en", out name)) return name;
            return Translations.Values.FirstOrDefault() ?? string.Empty;
        }

        public static CSharpFunctionalExtensions.Result<Breed, Error> Create(Guid id, Dictionary<string, string> translations)
        {
            if (translations == null || translations.Count == 0)
                return Error.Validation("breed.translations_empty", "Переводы породы не могут быть пустыми.");

            foreach (var (locale, name) in translations)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Error.Validation("breed.name_is_empty", $"Название породы для локали '{locale}' не может быть пустым.");

                if (name.Length > MAX_NAME_LENGTH)
                    return Error.Validation("breed.name_too_long",
                        $"Название породы для локали '{locale}' не должно превышать {MAX_NAME_LENGTH} символов.");
            }

            return new Breed(id, translations.ToDictionary(k => k.Key, v => v.Value.Trim()));
        }
    }
}