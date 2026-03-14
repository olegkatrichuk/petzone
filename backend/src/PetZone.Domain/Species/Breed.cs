using System;
using CSharpFunctionalExtensions;

namespace PetZone.Domain.Species
{
    // Порода животного
    public class Breed : Entity<Guid>
    {
        public string Name { get; private set; }

        public Breed(Guid id, string name) : base(id)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Название породы не может быть пустым.");
            
            Name = name;
        }
    }
}