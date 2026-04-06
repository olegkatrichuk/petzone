using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models
{
    public class Volunteer : SoftDeletableEntity<Guid>
    {
        public const int MaxGeneralDescriptionLength = 2000;

        public const int MaxPhotoPathLength = 500;

        public FullName Name { get; private set; }
        public Email Email { get; private set; }
        public string GeneralDescription { get; private set; }
        public Experience Experience { get; private set; }
        public PhoneNumber Phone { get; private set; }
        public string? PhotoPath { get; private set; }

        private readonly List<SocialNetwork> _socialNetworks = new();
        public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks.AsReadOnly();

        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        private readonly List<Pet> _pets = new();
        public IReadOnlyList<Pet> Pets => _pets.AsReadOnly();

        public override void Delete()
        {
            base.Delete();
            foreach (var pet in _pets)
                pet.Delete();
        }

        public override void Restore()
        {
            base.Restore();
            foreach (var pet in _pets)
                pet.Restore();
        }

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

        private Volunteer()
        {
            Name = null!;
            Email = null!;
            GeneralDescription = null!;
            Experience = null!;
            Phone = null!;
        }

        public static Result<Volunteer, Error> Create(
            Guid id, FullName name, Email email, string generalDescription,
            Experience experience, PhoneNumber phone)
        {
            if (string.IsNullOrWhiteSpace(generalDescription))
                return Error.Validation("volunteer.description_is_empty", "Общее описание волонтера обязательно.");

            if (generalDescription.Length > MaxGeneralDescriptionLength)
                return Error.Validation("volunteer.description_too_long",
                    $"Описание не должно превышать {MaxGeneralDescriptionLength} символов.");

            return new Volunteer(id, name, email, generalDescription.Trim(), experience, phone);
        }

        public void UpdatePhoto(string? photoPath) => PhotoPath = photoPath;

        public int CountPetsFoundHome() => _pets.Count(p => p.Status == HelpStatus.FoundHome);
        public int CountPetsLookingForHome() => _pets.Count(p => p.Status == HelpStatus.LookingForHome);
        public int CountPetsInTreatment() => _pets.Count(p => p.Status == HelpStatus.NeedsHelp);

        public Result<Volunteer, Error> AddPet(Pet pet)
        {
            if (_pets.Contains(pet))
                return Error.Validation("volunteer.pet_already_exists", "Питомец уже добавлен.");

            pet.SetPosition(_pets.Count + 1);
            _pets.Add(pet);

            return this;
        }

        public Result<Volunteer, Error> UpdateMainInfo(
            FullName name, Email email, string generalDescription,
            Experience experience, PhoneNumber phone)
        {
            if (string.IsNullOrWhiteSpace(generalDescription))
                return Error.Validation("volunteer.description_is_empty", "Описание обязательно.");

            if (generalDescription.Length > MaxGeneralDescriptionLength)
                return Error.Validation("volunteer.description_too_long",
                    $"Описание не должно превышать {MaxGeneralDescriptionLength} символов.");

            Name = name;
            Email = email;
            GeneralDescription = generalDescription.Trim();
            Experience = experience;
            Phone = phone;

            return this;
        }

        public void UpdateSocialNetworks(IEnumerable<SocialNetwork> socialNetworks)
        {
            _socialNetworks.Clear();
            _socialNetworks.AddRange(socialNetworks);
        }

        public void UpdateRequisites(IEnumerable<Requisite> requisites)
        {
            _requisites.Clear();
            _requisites.AddRange(requisites);
        }

        public Result<Volunteer, Error> RemovePet(Pet pet)
        {
            if (!_pets.Contains(pet))
                return Error.Validation("volunteer.pet_not_found", "Питомец не найден.");

            _pets.Remove(pet);
            RecalculatePositions();

            return this;
        }

        public Result<Volunteer, Error> MovePet(Pet pet, int newPosition)
        {
            if (!_pets.Contains(pet))
                return Error.Validation("volunteer.pet_not_found", "Питомец не найден.");

            if (newPosition < 1 || newPosition > _pets.Count)
                return Error.Validation("volunteer.invalid_position",
                    $"Позиция должна быть от 1 до {_pets.Count}.");

            if (pet.Position == newPosition)
                return this;

            var currentPosition = pet.Position;

            if (newPosition < currentPosition)
            {
                foreach (var p in _pets.Where(p => p.Position >= newPosition && p.Position < currentPosition))
                    p.SetPosition(p.Position + 1);
            }
            else
            {
                foreach (var p in _pets.Where(p => p.Position > currentPosition && p.Position <= newPosition))
                    p.SetPosition(p.Position - 1);
            }

            pet.SetPosition(newPosition);

            return this;
        }

        public Result<Volunteer, Error> MovePetToFirst(Pet pet) => MovePet(pet, 1);
        public Result<Volunteer, Error> MovePetToLast(Pet pet) => MovePet(pet, _pets.Count);

        public Result<Volunteer, Error> AddSocialNetwork(SocialNetwork network)
        {
            if (!_socialNetworks.Contains(network))
                _socialNetworks.Add(network);
            return this;
        }

        public Result<Volunteer, Error> AddRequisite(Requisite requisite)
        {
            if (!_requisites.Contains(requisite))
                _requisites.Add(requisite);
            return this;
        }

        private void RecalculatePositions()
        {
            var sorted = _pets.OrderBy(p => p.Position).ToList();
            for (int i = 0; i < sorted.Count; i++)
                sorted[i].SetPosition(i + 1);
        }
    }
}
