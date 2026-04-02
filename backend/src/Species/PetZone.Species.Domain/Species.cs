using PetZone.SharedKernel;

namespace PetZone.Species.Domain
{
    public class Species : Entity<Guid>
    {
        public const int MAX_NAME_LENGTH = 100;

        public Dictionary<string, string> Translations { get; private set; } = new();

        private readonly List<Breed> _breeds = new();
        public IReadOnlyList<Breed> Breeds => _breeds.AsReadOnly();

        private Species(Guid id, Dictionary<string, string> translations) : base(id)
        {
            Translations = translations;
        }

        private Species() { }

        public string GetName(string locale)
        {
            if (Translations.TryGetValue(locale, out var name)) return name;
            if (Translations.TryGetValue("uk", out name)) return name;
            if (Translations.TryGetValue("en", out name)) return name;
            return Translations.Values.FirstOrDefault() ?? string.Empty;
        }

        public static CSharpFunctionalExtensions.Result<Species, Error> Create(Guid id, Dictionary<string, string> translations)
        {
            if (translations == null || translations.Count == 0)
                return Error.Validation("species.translations_empty", "Переводы вида не могут быть пустыми.");

            foreach (var (locale, name) in translations)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Error.Validation("species.name_is_empty", $"Название вида для локали '{locale}' не может быть пустым.");

                if (name.Length > MAX_NAME_LENGTH)
                    return Error.Validation("species.name_too_long",
                        $"Название вида для локали '{locale}' не должно превышать {MAX_NAME_LENGTH} символов.");
            }

            return new Species(id, translations.ToDictionary(k => k.Key, v => v.Value.Trim()));
        }

        public CSharpFunctionalExtensions.Result<Species, Error> AddBreed(Breed breed)
        {
            if (breed == null)
                return Error.Validation("species.breed_is_null", "Порода не может быть пустой.");

            if (_breeds.Any(b => b.Id == breed.Id))
                return Error.Conflict("species.breed_already_exists", "Порода уже существует в этом виде.");

            _breeds.Add(breed);
            return this;
        }

        public CSharpFunctionalExtensions.Result<Species, Error> RemoveBreed(Breed breed)
        {
            if (breed == null)
                return Error.Validation("species.breed_is_null", "Порода не может быть пустой.");

            _breeds.Remove(breed);
            return this;
        }
    }
}