using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models
{
    // --- НОВЫЕ VALUE OBJECTS ---

    public abstract class FullName : ValueObject
    {
        private string FirstName { get; }
        private string LastName { get; }
        private string Patronymic { get; }

        protected FullName(string firstName, string lastName, string patronymic = "")
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("Имя обязательно.");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Фамилия обязательна.");
            
            FirstName = firstName;
            LastName = lastName;
            Patronymic = patronymic;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
            yield return Patronymic;
        }
    }

    public abstract class Email : ValueObject
    {
        private string Value { get; }

        protected Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@")) 
                throw new ArgumentException("Некорректный формат Email.");
            
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public abstract class Experience : ValueObject
    {
        private int Years { get; }

        protected Experience(int years)
        {
            if (years < 0) throw new ArgumentException("Опыт не может быть отрицательным.");
            Years = years;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Years;
        }
    }

    public abstract class SocialNetwork : ValueObject
    {
        private string Name { get; }
        private string Link { get; }

        protected SocialNetwork(string name, string link)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название соц. сети обязательно.");
            if (string.IsNullOrWhiteSpace(link)) throw new ArgumentException("Ссылка обязательна.");
            
            Name = name;
            Link = link;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Link;
        }
    }

    // --- БОГАТАЯ СУЩНОСТЬ VOLUNTEER ---

    public class Volunteer(
        Guid id,
        FullName name,
        Email email,
        string generalDescription,
        Experience experience,
        PhoneNumber phone)
        : Entity<Guid>(id)
    {
        public FullName Name { get; private set; } = name;
        public Email Email { get; private set; } = email;
        public string GeneralDescription { get; private set; } = generalDescription;
        public Experience Experience { get; private set; } = experience;
        public PhoneNumber Phone { get; private set; } = phone;

        // Инкапсулированные коллекции
        private readonly List<SocialNetwork> _socialNetworks = new();
        public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks.AsReadOnly();

        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        private readonly List<Pet> _pets = new();
        public IReadOnlyList<Pet> Pets => _pets.AsReadOnly();

        // --- МЕТОДЫ ПОВЕДЕНИЯ (Domain Methods) ---

        // 1. Метод: Количество животных, нашедших дом
        public int CountPetsFoundHome()
        {
            return _pets.Count(p => p.Status == HelpStatus.FoundHome);
        }

        // 2. Метод: Количество животных, ищущих дом
        public int CountPetsLookingForHome()
        {
            return _pets.Count(p => p.Status == HelpStatus.LookingForHome);
        }

        // 3. Метод: Количество животных на лечении (нуждаются в помощи)
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