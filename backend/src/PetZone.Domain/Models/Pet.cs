
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

    public abstract class Address(string city, string street) : ValueObject
    {
        private string City { get; } = city;
        private string Street { get; } = street;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return City;
            yield return Street;
        }
    }

    public abstract class Weight : ValueObject
    {
        private double Value { get; }

        protected Weight(double value)
        {
            if (value <= 0) throw new ArgumentException("Вес должен быть больше 0.");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public abstract class Height : ValueObject
    {
        private double Value { get; }

        protected Height(double value)
        {
            if (value <= 0) throw new ArgumentException("Рост должен быть больше 0.");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public abstract class PhoneNumber : ValueObject
    {
        private string Value { get; }

        protected PhoneNumber(string value)
        {
            // Здесь должна быть логика валидации формата номера телефона (Regex и т.д.)
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Номер не может быть пустым.");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public abstract class HealthInfo(string generalDescription, string dietOrAllergies = "") : ValueObject
    {
        private string GeneralDescription { get; } = generalDescription;
        private string DietOrAllergies { get; } = dietOrAllergies;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GeneralDescription;
            yield return DietOrAllergies;
        }
    }

    public abstract class Requisite : ValueObject
    {
        private string Name { get; }
        private string Description { get; }

        protected Requisite(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название реквизита обязательно.");
            Name = name;
            Description = description;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Description;
        }
    }

    // --- БОГАТАЯ СУЩНОСТЬ (Entity) ---

    public abstract class Pet(
        Guid id,
        string nickname,
        string generalDescription,
        string color,
        HealthInfo health,
        Address location,
        Weight weight,
        Height height,
        PhoneNumber ownerPhone,
        bool isCastrated,
        DateTime dateOfBirth,
        bool isVaccinated,
        HelpStatus status,
        string microchipNumber,
        Guid? volunteerId,
        string adoptionConditions,
        SpeciesBreed speciesBreedInfo)
        : Entity<Guid>(id)
    {
        // Приватные сеттеры, чтобы избежать изменения состояния извне
        public string Nickname { get; private set; } = nickname;
        public SpeciesBreed SpeciesBreedInfo { get; private set; } = speciesBreedInfo;
        public string GeneralDescription { get; private set; } = generalDescription;
        public string Color { get; private set; } = color;
        public HealthInfo Health { get; private set; } = health;
        public Address Location { get; private set; } = location;
        public Weight Weight { get; private set; } = weight;
        public Height Height { get; private set; } = height;
        public PhoneNumber OwnerPhone { get; private set; } = ownerPhone;
        public bool IsCastrated { get; private set; } = isCastrated;
        public DateTime DateOfBirth { get; private set; } = dateOfBirth;
        public bool IsVaccinated { get; private set; } = isVaccinated;
        public HelpStatus Status { get; private set; } = status;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow; // Инициализируем датой создания

        // Мои 2 дополнительных свойства
        public string? AdoptionConditions { get; private set; } = adoptionConditions;
        public string? MicrochipNumber { get; private set; } = microchipNumber;
        public Guid? VolunteerId { get; private set; } = volunteerId;

        // Инкапсулированная коллекция для реквизитов
        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        // --- ПОВЕДЕНИЕ (Domain Methods) ---
        // Именно эти методы делают модель "богатой" (Rich)

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
            
            // Защита инвариантов: можно добавить проверку, нет ли уже такого реквизита
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
// Value Object для связи питомца со справочником видов и пород
public abstract class SpeciesBreed : ValueObject
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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SpeciesId;
        yield return BreedId;
    }
}