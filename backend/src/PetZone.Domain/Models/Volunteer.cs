using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models
{
    // --- VALUE OBJECTS ---

    // --- БОГАТАЯ СУЩНОСТЬ VOLUNTEER ---

    // Убрали первичный конструктор
    public class Volunteer : Entity<Guid>
    {
        public FullName Name { get; private set; }
        public Email Email { get; private set; }
        public string GeneralDescription { get; private set; }
        public Experience Experience { get; private set; }
        public PhoneNumber Phone { get; private set; } // Берется из общих VO (где у вас Pet.cs)

        // Инкапсулированные коллекции
        private readonly List<SocialNetwork> _socialNetworks = new();
        public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks.AsReadOnly();

        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        private readonly List<Pet> _pets = new();
        public IReadOnlyList<Pet> Pets => _pets.AsReadOnly();

        // 1. ОСНОВНОЙ КОНСТРУКТОР
        public Volunteer(
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

        // 2. ПУСТОЙ ПРИВАТНЫЙ КОНСТРУКТОР ДЛЯ EF CORE
        private Volunteer() { }

        // --- МЕТОДЫ ПОВЕДЕНИЯ (Domain Methods) ---

        public int CountPetsFoundHome()
        {
            return _pets.Count(p => p.Status == HelpStatus.FoundHome);
        }

        public int CountPetsLookingForHome()
        {
            return _pets.Count(p => p.Status == HelpStatus.LookingForHome);
        }

        public int CountPetsInTreatment()
        {
            return _pets.Count(p => p.Status == HelpStatus.NeedsHelp);
        }

        // --- МЕТОДЫ УПРАВЛЕНИЯ КОЛЛЕКЦИЯМИ ---

        public void AddPet(Pet pet)
        {
            if (pet == null) throw new ArgumentNullException(nameof(pet));
            if (!_pets.Contains(pet))
            {
                _pets.Add(pet);
            }
        }

        public void AddSocialNetwork(SocialNetwork network)
        {
            if (network == null) throw new ArgumentNullException(nameof(network));
            if (!_socialNetworks.Contains(network))
            {
                _socialNetworks.Add(network);
            }
        }

        public void AddRequisite(Requisite requisite)
        {
            if (requisite == null) throw new ArgumentNullException(nameof(requisite));
            if (!_requisites.Contains(requisite))
            {
                _requisites.Add(requisite);
            }
        }
    }
}