using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared; // Обязательно для класса Error

namespace PetZone.Domain.Models
{
    public enum HelpStatus
    {
        NeedsHelp,
        LookingForHome,
        FoundHome
    }

    public class Pet : Entity<Guid>
    {
        // --- 1. ПУБЛИЧНЫЕ КОНСТАНТЫ ДЛЯ ВАЛИДАЦИИ ---
        public const int MAX_NICKNAME_LENGTH = 100;
        public const int MAX_GENERAL_DESCRIPTION_LENGTH = 2000;
        public const int MAX_COLOR_LENGTH = 50;
        public const int MAX_MICROCHIP_NUMBER_LENGTH = 50;
        public const int MAX_ADOPTION_CONDITIONS_LENGTH = 1000;

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

        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        // --- 2. КОНСТРУКТОР ТЕПЕРЬ ПРИВАТНЫЙ ---
        private Pet(
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

        private Pet()
        {
        } // Для EF Core

        // --- 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ ---
        public static Result<Pet, Error> Create(
            Guid id, string nickname, string generalDescription, string color,
            HealthInfo health, Address location, Weight weight, Height height,
            PhoneNumber ownerPhone, bool isCastrated, DateTime dateOfBirth,
            bool isVaccinated, HelpStatus status, string? microchipNumber,
            Guid? volunteerId, string? adoptionConditions, SpeciesBreed speciesBreedInfo)
        {
            // Валидация клички
            if (string.IsNullOrWhiteSpace(nickname))
                return Error.Validation("pet.nickname_is_empty", "Кличка питомца не может быть пустой.");
            if (nickname.Length > MAX_NICKNAME_LENGTH)
                return Error.Validation("pet.nickname_too_long",
                    $"Кличка не должна превышать {MAX_NICKNAME_LENGTH} символов.");

            // Валидация описания
            if (string.IsNullOrWhiteSpace(generalDescription))
                return Error.Validation("pet.description_is_empty", "Описание питомца обязательно.");
            if (generalDescription.Length > MAX_GENERAL_DESCRIPTION_LENGTH)
                return Error.Validation("pet.description_too_long",
                    $"Описание не должно превышать {MAX_GENERAL_DESCRIPTION_LENGTH} символов.");

            // Валидация цвета
            if (string.IsNullOrWhiteSpace(color))
                return Error.Validation("pet.color_is_empty", "Цвет питомца обязателен.");
            if (color.Length > MAX_COLOR_LENGTH)
                return Error.Validation("pet.color_too_long", $"Цвет не должен превышать {MAX_COLOR_LENGTH} символов.");

            // Опциональные поля (чип и условия)
            if (!string.IsNullOrWhiteSpace(microchipNumber) && microchipNumber.Length > MAX_MICROCHIP_NUMBER_LENGTH)
                return Error.Validation("pet.microchip_too_long",
                    $"Номер микрочипа не должен превышать {MAX_MICROCHIP_NUMBER_LENGTH} символов.");

            if (!string.IsNullOrWhiteSpace(adoptionConditions) &&
                adoptionConditions.Length > MAX_ADOPTION_CONDITIONS_LENGTH)
                return Error.Validation("pet.conditions_too_long",
                    $"Условия пристройства не должны превышать {MAX_ADOPTION_CONDITIONS_LENGTH} символов.");

            // Если всё отлично, собираем сущность
            return new Pet(
                id, nickname.Trim(), generalDescription.Trim(), color.Trim(),
                health, location, weight, height, ownerPhone, isCastrated,
                dateOfBirth, isVaccinated, status,
                microchipNumber?.Trim(), volunteerId, adoptionConditions?.Trim(), speciesBreedInfo
            );
        }

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

        // 4. ИЗБАВЛЯЕМСЯ ОТ THROW В ДОМЕННОМ МЕТОДЕ
        public Result<Pet, Error> AddRequisite(Requisite requisite)
        {
            if (requisite == null)
            {
                return Error.Validation("pet.requisite_is_null", "Реквизит не может быть пустым.");
            }

            if (!_requisites.Contains(requisite))
            {
                _requisites.Add(requisite);
            }

            return this; // Возвращаем саму сущность при успехе
        }

        public void PerformMedicalProcedures(bool vaccinated, bool castrated)
        {
            IsVaccinated = vaccinated;
            IsCastrated = castrated;
        }
    }
}