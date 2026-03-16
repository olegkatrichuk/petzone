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