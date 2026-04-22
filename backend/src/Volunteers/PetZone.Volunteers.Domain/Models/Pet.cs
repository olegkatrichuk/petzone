using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models
{
    public enum HelpStatus
    {
        NeedsHelp,
        LookingForHome,
        FoundHome
    }

    public class Pet : SoftDeletableEntity<Guid>
    {
        public const int MAX_NICKNAME_LENGTH = 100;
        public const int MAX_PHOTOS_COUNT = 5;
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
        public int Position { get; private set; }
        public string? ExternalId { get; private set; }
        public string? ExternalUrl { get; private set; }
        public string? Country { get; private set; }

        private readonly List<Requisite> _requisites = new();
        public IReadOnlyList<Requisite> Requisites => _requisites.AsReadOnly();

        private readonly List<PetPhoto> _photos = new();
        public IReadOnlyList<PetPhoto> Photos => _photos.AsReadOnly();

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
            Nickname = null!;
            SpeciesBreedInfo = null!;
            GeneralDescription = null!;
            Color = null!;
            Health = null!;
            Location = null!;
            Weight = null!;
            Height = null!;
            OwnerPhone = null!;
        }

        public static Result<Pet, Error> Create(
            Guid id, string nickname, string generalDescription, string color,
            HealthInfo health, Address location, Weight weight, Height height,
            PhoneNumber ownerPhone, bool isCastrated, DateTime dateOfBirth,
            bool isVaccinated, HelpStatus status, string? microchipNumber,
            Guid? volunteerId, string? adoptionConditions, SpeciesBreed speciesBreedInfo)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                return Error.Validation("pet.nickname_is_empty", "Кличка питомца не может быть пустой.");
            if (nickname.Length > MAX_NICKNAME_LENGTH)
                return Error.Validation("pet.nickname_too_long",
                    $"Кличка не должна превышать {MAX_NICKNAME_LENGTH} символов.");

            if (string.IsNullOrWhiteSpace(generalDescription))
                return Error.Validation("pet.description_is_empty", "Описание питомца обязательно.");
            if (generalDescription.Length > MAX_GENERAL_DESCRIPTION_LENGTH)
                return Error.Validation("pet.description_too_long",
                    $"Описание не должно превышать {MAX_GENERAL_DESCRIPTION_LENGTH} символов.");

            if (string.IsNullOrWhiteSpace(color))
                return Error.Validation("pet.color_is_empty", "Цвет питомца обязателен.");
            if (color.Length > MAX_COLOR_LENGTH)
                return Error.Validation("pet.color_too_long",
                    $"Цвет не должен превышать {MAX_COLOR_LENGTH} символов.");

            if (!string.IsNullOrWhiteSpace(microchipNumber) && microchipNumber.Length > MAX_MICROCHIP_NUMBER_LENGTH)
                return Error.Validation("pet.microchip_too_long",
                    $"Номер микрочипа не должен превышать {MAX_MICROCHIP_NUMBER_LENGTH} символов.");

            if (!string.IsNullOrWhiteSpace(adoptionConditions) &&
                adoptionConditions.Length > MAX_ADOPTION_CONDITIONS_LENGTH)
                return Error.Validation("pet.conditions_too_long",
                    $"Условия пристройства не должны превышать {MAX_ADOPTION_CONDITIONS_LENGTH} символов.");

            return new Pet(
                id, nickname.Trim(), generalDescription.Trim(), color.Trim(),
                health, location, weight, height, ownerPhone, isCastrated,
                dateOfBirth, isVaccinated, status,
                microchipNumber?.Trim(), volunteerId, adoptionConditions?.Trim(), speciesBreedInfo);
        }

        public Result<Pet, Error> Update(
            string nickname, string generalDescription, string color,
            HealthInfo health, Address location, Weight weight, Height height,
            PhoneNumber ownerPhone, bool isCastrated, DateTime dateOfBirth,
            bool isVaccinated, HelpStatus status, string? microchipNumber,
            string? adoptionConditions, SpeciesBreed speciesBreed)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                return Error.Validation("pet.nickname_is_empty", "Кличка питомца не может быть пустой.");
            if (nickname.Length > MAX_NICKNAME_LENGTH)
                return Error.Validation("pet.nickname_too_long",
                    $"Кличка не должна превышать {MAX_NICKNAME_LENGTH} символов.");

            if (string.IsNullOrWhiteSpace(generalDescription))
                return Error.Validation("pet.description_is_empty", "Описание питомца обязательно.");
            if (generalDescription.Length > MAX_GENERAL_DESCRIPTION_LENGTH)
                return Error.Validation("pet.description_too_long",
                    $"Описание не должно превышать {MAX_GENERAL_DESCRIPTION_LENGTH} символов.");

            if (string.IsNullOrWhiteSpace(color))
                return Error.Validation("pet.color_is_empty", "Цвет питомца обязателен.");

            Nickname = nickname.Trim();
            GeneralDescription = generalDescription.Trim();
            Color = color.Trim();
            Health = health;
            Location = location;
            Weight = weight;
            Height = height;
            OwnerPhone = ownerPhone;
            IsCastrated = isCastrated;
            DateOfBirth = dateOfBirth;
            IsVaccinated = isVaccinated;
            Status = status;
            MicrochipNumber = microchipNumber?.Trim();
            AdoptionConditions = adoptionConditions?.Trim();
            SpeciesBreedInfo = speciesBreed;

            return this;
        }

        public Result<Pet, Error> UpdateStatus(HelpStatus newStatus)
        {
            if (newStatus == HelpStatus.FoundHome)
                return Error.Validation("pet.invalid_status",
                    "Волонтёр не может установить статус 'Нашёл дом'.");

            Status = newStatus;
            return this;
        }

        public void UpdateHealth(HealthInfo newHealthInfo, Weight newWeight)
        {
            Health = newHealthInfo;
            Weight = newWeight;
        }

        public void MoveToNewAddress(Address newAddress)
        {
            Location = newAddress;
        }

        public void PerformMedicalProcedures(bool vaccinated, bool castrated)
        {
            IsVaccinated = vaccinated;
            IsCastrated = castrated;
        }

        public Result<Pet, Error> AddPhoto(PetPhoto photo)
        {
            if (photo == null)
                return Error.Validation("pet.photo_is_null", "Фото не может быть пустым.");

            if (_photos.Count >= MAX_PHOTOS_COUNT)
                return Error.Validation("pet.photos_limit_exceeded",
                    $"Нельзя добавить больше {MAX_PHOTOS_COUNT} фото для одного питомца.");

            if (_photos.Any(p => p.FilePath == photo.FilePath))
                return Error.Validation("pet.photo_already_exists", "Фото уже добавлено.");

            _photos.Add(photo);
            return this;
        }

        public Result<Pet, Error> RemovePhoto(PetPhoto photo)
        {
            if (photo == null)
                return Error.Validation("pet.photo_is_null", "Фото не может быть пустым.");

            var existing = _photos.FirstOrDefault(p => p.FilePath == photo.FilePath);
            if (existing == null)
                return Error.NotFound("pet.photo_not_found", "Фото не найдено.");

            _photos.Remove(existing);
            return this;
        }

        public void UpdatePhotos(IEnumerable<PetPhoto> photos)
        {
            _photos.Clear();
            _photos.AddRange(photos);
        }

        public Result<Pet, Error> SetMainPhoto(string filePath)
        {
            var photo = _photos.FirstOrDefault(p => p.FilePath == filePath);
            if (photo is null)
                return Error.NotFound("pet.photo_not_found", "Фото не найдено.");

            var updatedPhotos = _photos
                .Select(p => PetPhoto.Create(p.FilePath, p.FilePath == filePath).Value)
                .ToList();

            _photos.Clear();
            _photos.AddRange(updatedPhotos);

            return this;
        }

        public Result<Pet, Error> AddRequisite(Requisite requisite)
        {
            if (requisite == null)
                return Error.Validation("pet.requisite_is_null", "Реквизит не может быть пустым.");

            if (!_requisites.Contains(requisite))
                _requisites.Add(requisite);

            return this;
        }

        internal void SetPosition(int position) => Position = position;

        public void SetExternalId(string externalId) => ExternalId = externalId;

        public void SetExternalUrl(string url) => ExternalUrl = url;

        public void SetCountry(string country) => Country = country;
    }
}