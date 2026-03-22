using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

// Подключаем ваши ошибки

namespace PetZone.Species.Domain
{
    // Вид животного (Корень агрегации)
    public class Species : Entity<Guid>
    {
        // 1. ПУБЛИЧНАЯ КОНСТАНТА ДЛЯ EF CORE
        public const int MAX_NAME_LENGTH = 100;

        public string Name { get; private set; }

        // Инкапсулированная коллекция пород
        private readonly List<Breed> _breeds = new();
        public IReadOnlyList<Breed> Breeds => _breeds.AsReadOnly();

        // 2. ПРИВАТНЫЙ КОНСТРУКТОР (без throw)
        private Species(Guid id, string name) : base(id)
        {
            Name = name;
        }

        private Species() { } // Для EF Core

        // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
        public static Result<Species, Error> Create(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Error.Validation("species.name_is_empty", "Название вида не может быть пустым.");

            if (name.Length > MAX_NAME_LENGTH)
                return Error.Validation("species.name_too_long", $"Название вида не должно превышать {MAX_NAME_LENGTH} символов.");

            return new Species(id, name.Trim());
        }

        // --- ПОВЕДЕНИЕ (Domain Methods) ---

        // 4. МЕНЯЕМ THROW НА RESULT
        public Result<Species, Error> AddBreed(Breed breed)
        {
            if (breed == null) 
                return Error.Validation("species.breed_is_null", "Порода не может быть пустой.");
            
            // Проверка на дубликаты (возвращаем конфликт вместо исключения)
            if (_breeds.Any(b => b.Name.Equals(breed.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Error.Conflict("species.breed_already_exists", $"Порода с именем '{breed.Name}' уже существует в этом виде.");
            }

            _breeds.Add(breed);
            return this; // Возвращаем саму сущность при успехе
        }

        public Result<Species, Error> RemoveBreed(Breed breed)
        {
            if (breed == null) 
                return Error.Validation("species.breed_is_null", "Порода не может быть пустой.");
                
            _breeds.Remove(breed);
            return this;
        }
    }
}