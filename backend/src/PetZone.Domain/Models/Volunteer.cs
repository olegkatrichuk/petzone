using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared; // Для класса Error

namespace PetZone.Domain.Models
{
    public class Volunteer : Entity<Guid>
    {
        // 1. ПУБЛИЧНАЯ КОНСТАНТА
        public const int MAX_GENERAL_DESCRIPTION_LENGTH = 2000;

        public FullName Name { get; private set; }
        public Email Email { get; private set; }
        public string GeneralDescription { get; private set; }
        public Experience Experience { get; private set; }
        public PhoneNumber Phone { get; private set; }

        // Инкапсулированные коллекции
        private readonly List<SocialNetwork> _socialNetworks = new();
        public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks.AsReadOnly();

        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        private readonly List<Pet> _pets = new();
        public IReadOnlyList<Pet> Pets => _pets.AsReadOnly();

        // 2. ПРИВАТНЫЙ ОСНОВНОЙ КОНСТРУКТОР
        private Volunteer(
            Guid id, FullName name, Email email, string generalDescription, 
            Experience experience, PhoneNumber phone) 
            : base(id)
        {
            Name = name;
            Email = email;
            GeneralDescription = generalDescription;
            Experience = experience;
            Phone = phone;
        }

        private Volunteer() { } // Для EF Core

        // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
        public static Result<Volunteer, Error> Create(
            Guid id, FullName name, Email email, string generalDescription, 
            Experience experience, PhoneNumber phone)
        {
            if (string.IsNullOrWhiteSpace(generalDescription))
                return Error.Validation("volunteer.description_is_empty", "Общее описание волонтера обязательно.");

            if (generalDescription.Length > MAX_GENERAL_DESCRIPTION_LENGTH)
                return Error.Validation("volunteer.description_too_long", $"Описание не должно превышать {MAX_GENERAL_DESCRIPTION_LENGTH} символов.");

            return new Volunteer(id, name, email, generalDescription.Trim(), experience, phone);
        }

        // --- МЕТОДЫ ПОВЕДЕНИЯ (Domain Methods) ---

        public int CountPetsFoundHome() => _pets.Count(p => p.Status == HelpStatus.FoundHome);
        public int CountPetsLookingForHome() => _pets.Count(p => p.Status == HelpStatus.LookingForHome);
        public int CountPetsInTreatment() => _pets.Count(p => p.Status == HelpStatus.NeedsHelp);

        // --- МЕТОДЫ УПРАВЛЕНИЯ КОЛЛЕКЦИЯМИ (Без throw!) ---

        public Result<Volunteer, Error> AddPet(Pet pet)
        {
            if (pet == null) 
                return Error.Validation("volunteer.pet_is_null", "Питомец не может быть пустым.");
                
            if (!_pets.Contains(pet))
                _pets.Add(pet);
                
            return this;
        }

        public Result<Volunteer, Error> AddSocialNetwork(SocialNetwork network)
        {
            if (network == null) 
                return Error.Validation("volunteer.socialnetwork_is_null", "Соцсеть не может быть пустой.");
                
            if (!_socialNetworks.Contains(network))
                _socialNetworks.Add(network);
                
            return this;
        }

        public Result<Volunteer, Error> AddRequisite(Requisite requisite)
        {
            if (requisite == null) 
                return Error.Validation("volunteer.requisite_is_null", "Реквизит не может быть пустым.");
                
            if (!_requisites.Contains(requisite))
                _requisites.Add(requisite);
                
            return this;
        }
    }
}