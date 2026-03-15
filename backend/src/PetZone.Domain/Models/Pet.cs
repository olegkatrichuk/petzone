using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models
{
    // --- ПЕРЕЧИСЛЕНИЯ (Enums) ---
    public enum HelpStatus
    {
        NeedsHelp,
        LookingForHome,
        FoundHome
    }

    // --- VALUE OBJECTS ---

    public class Address : ValueObject
    {
        private string City { get; }
        private string Street { get; }

        public Address(string city, string street)
        {
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("Город обязателен.");
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Улица обязательна.");
            
            City = city;
            Street = street;
        }

        private Address() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return City;
            yield return Street;
        }
    }

    public class Weight : ValueObject
    {
        private double Value { get; }

        public Weight(double value)
        {
            if (value <= 0) throw new ArgumentException("Вес должен быть больше 0.");
            Value = value;
        }

        private Weight() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class Height : ValueObject
    {
        private double Value { get; }

        public Height(double value)
        {
            if (value <= 0) throw new ArgumentException("Рост должен быть больше 0.");
            Value = value;
        }

        private Height() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class PhoneNumber : ValueObject
    {
        private string Value { get; }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Номер не может быть пустым.");
            Value = value;
        }

        private PhoneNumber() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class HealthInfo : ValueObject
    {
        private string GeneralDescription { get; }
        private string DietOrAllergies { get; }

        public HealthInfo(string generalDescription, string dietOrAllergies = "")
        {
            if (string.IsNullOrWhiteSpace(generalDescription)) throw new ArgumentException("Описание здоровья обязательно.");
            GeneralDescription = generalDescription;
            DietOrAllergies = dietOrAllergies;
        }

        private HealthInfo() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GeneralDescription;
            yield return DietOrAllergies;
        }
    }

    public class Requisite : ValueObject
    {
        private string Name { get; }
        private string Description { get; }

        public Requisite(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название реквизита обязательно.");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Описание реквизита обязательно.");
            
            Name = name;
            Description = description;
        }

        private Requisite() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Description;
        }
    }

    public class SpeciesBreed : ValueObject
    {
        private Guid SpeciesId { get; }
        private Guid BreedId { get; }

        public SpeciesBreed(Guid speciesId, Guid breedId)
        {
            if (speciesId == Guid.Empty) throw new ArgumentException("SpeciesId не может быть пустым.");
            if (breedId == Guid.Empty) throw new ArgumentException("BreedId не может быть пустым.");

            SpeciesId = speciesId;
            BreedId = breedId;
        }

        private SpeciesBreed() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SpeciesId;
            yield return BreedId;
        }
    }

    // --- БОГАТАЯ СУЩНОСТЬ (Entity) ---

    public class Pet : Entity<Guid>
    {
        // Приватные сеттеры
        public string Nickname { get; private set; }
        public SpeciesBreed SpeciesBreedInfo { get; private set; }
        public string GeneralDescription { get; private set; }
        public string Color { get; private set; }
        public HealthInfo Health { get; private set; }
        public Address Location { get; private set; }
        public Weight Weight { get; private set; }
        public Height Height { get; private set; }
        public PhoneNumber OwnerPhone { get; private set; }
        public bool IsCastrated { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public bool IsVaccinated { get; private set; }
        public HelpStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public string? AdoptionConditions { get; private set; }
        public string? MicrochipNumber { get; private set; }
        public Guid? VolunteerId { get; private set; }

        // Инкапсулированная коллекция
        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        // 1. ОСНОВНОЙ КОНСТРУКТОР (публичный, для использования в коде)
        public Pet(
            Guid id, string nickname, string generalDescription, string color,
            HealthInfo health, Address location, Weight weight, Height height,
            PhoneNumber ownerPhone, bool isCastrated, DateTime dateOfBirth,
            bool isVaccinated, HelpStatus status, string? microchipNumber,
            Guid? volunteerId, string? adoptionConditions, SpeciesBreed speciesBreedInfo)
            : base(id)
        {
            Nickname = nickname;
            GeneralDescription = generalDescription;
            Color = color;
            Health = health;
            Location = location;
            Weight = weight;
            Height = height;
            OwnerPhone = ownerPhone;
            IsCastrated = isCastrated;
            DateOfBirth = dateOfBirth;
            IsVaccinated = isVaccinated;
            Status = status;
            MicrochipNumber = microchipNumber;
            VolunteerId = volunteerId;
            AdoptionConditions = adoptionConditions;
            SpeciesBreedInfo = speciesBreedInfo;
            CreatedAt = DateTime.UtcNow;
        }

        // 2. ПУСТОЙ КОНСТРУКТОР ДЛЯ EF CORE (решает ошибку маппинга!)
        private Pet() { }

        // --- ПОВЕДЕНИЕ (Domain Methods) ---

        public void UpdateHealth(HealthInfo newHealthInfo, Weight newWeight)
        {
            Health = newHealthInfo;
            Weight = newWeight;
        }

        public void UpdateStatus(HelpStatus newStatus)
        {
            Status = newStatus;
        }

        public void MoveToNewAddress(Address newAddress)
        {
            Location = newAddress;
        }

        public void AddRequisite(Requisite requisite)
        {
            if (requisite == null) throw new ArgumentNullException(nameof(requisite));
            
            if (!_requisites.Contains(requisite))
            {
                _requisites.Add(requisite);
            }
        }

        public void PerformMedicalProcedures(bool vaccinated, bool castrated)
        {
            IsVaccinated = vaccinated;
            IsCastrated = castrated;
        }
    }
}