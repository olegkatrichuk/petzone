using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

// Не забываем ваши ошибки

namespace PetZone.Species.Domain
{
    // Порода животного
    public class Breed : Entity<Guid>
    {
        // 1. ПУБЛИЧНАЯ КОНСТАНТА
        public const int MAX_NAME_LENGTH = 100;

        public string Name { get; private set; }

        // 2. ПРИВАТНЫЙ КОНСТРУКТОР (без throw)
        private Breed(Guid id, string name) : base(id)
        {
            Name = name;
        }

        private Breed() { } // Для EF Core

        // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
        public static Result<Breed, Error> Create(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Error.Validation("breed.name_is_empty", "Название породы не может быть пустым.");
            }

            if (name.Length > MAX_NAME_LENGTH)
            {
                return Error.Validation("breed.name_too_long", $"Название породы не должно превышать {MAX_NAME_LENGTH} символов.");
            }

            return new Breed(id, name.Trim());
        }
    }
}