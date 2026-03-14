using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace PetZone.Domain.Species
{
    // Вид животного (Корень агрегации)
    public class Species : Entity<Guid>
    {
        public string Name { get; private set; }

        // Инкапсулированная коллекция пород
        private readonly List<Breed> _breeds = new();
        public IReadOnlyList<Breed> Breeds => _breeds.AsReadOnly();

        public Species(Guid id, string name) : base(id)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Название вида не может быть пустым.");
            
            Name = name;
        }

        // --- ПОВЕДЕНИЕ (Domain Methods) ---

        // Добавление новой породы к виду
        public void AddBreed(Breed breed)
        {
            if (breed == null) throw new ArgumentNullException(nameof(breed));
            
            // Проверка, что такой породы еще нет, чтобы избежать дубликатов
            if (_breeds.Any(b => b.Name.Equals(breed.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Порода с именем '{breed.Name}' уже существует в этом виде.");
            }

            _breeds.Add(breed);
        }

        // Удаление породы (например, если создали по ошибке)
        public void RemoveBreed(Breed breed)
        {
            if (breed == null) throw new ArgumentNullException(nameof(breed));
            _breeds.Remove(breed);
        }
    }
}