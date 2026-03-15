using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models
{
    // --- VALUE OBJECTS ---

    public class FullName : ValueObject
    {
        private string FirstName { get; }
        private string LastName { get; }
        private string Patronymic { get; }

        public FullName(string firstName, string lastName, string patronymic = "")
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("Имя обязательно.");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Фамилия обязательна.");
            
            FirstName = firstName;
            LastName = lastName;
            Patronymic = patronymic;
        }

        private FullName() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
            yield return Patronymic;
        }
    }

    public class Email : ValueObject
    {
        private string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@")) 
                throw new ArgumentException("Некорректный формат Email.");
            
            Value = value;
        }

        private Email() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class Experience : ValueObject
    {
        private int Years { get; }

        public Experience(int years)
        {
            if (years < 0) throw new ArgumentException("Опыт не может быть отрицательным.");
            Years = years;
        }

        private Experience() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Years;
        }
    }

    public class SocialNetwork : ValueObject
    {
        private string Name { get; }
        private string Link { get; }

        public SocialNetwork(string name, string link)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название соц. сети обязательно.");
            if (string.IsNullOrWhiteSpace(link)) throw new ArgumentException("Ссылка обязательна.");
            
            Name = name;
            Link = link;
        }

        private SocialNetwork() { } // Для EF Core

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Link;
        }
    }

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