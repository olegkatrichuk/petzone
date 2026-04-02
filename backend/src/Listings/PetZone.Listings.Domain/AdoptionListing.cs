using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Listings.Domain;

public class AdoptionListing : PetZone.SharedKernel.Entity<Guid>
{
    public const int MaxPhotos = 5;
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxColorLength = 100;
    public const int MaxCityLength = 150;
    public const int MaxPhoneLength = 30;

    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string UserEmail { get; private set; } = string.Empty;
    public string? UserPhone { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid SpeciesId { get; private set; }
    public Guid? BreedId { get; private set; }
    public int AgeMonths { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public bool Vaccinated { get; private set; }
    public bool Castrated { get; private set; }
    public List<string> Photos { get; private set; } = [];
    public ListingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AdoptionListing() { }

    private AdoptionListing(
        Guid id,
        Guid userId,
        string userName,
        string userEmail,
        string? userPhone,
        string title,
        string description,
        Guid speciesId,
        Guid? breedId,
        int ageMonths,
        string color,
        string city,
        bool vaccinated,
        bool castrated) : base(id)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        UserPhone = userPhone;
        Title = title;
        Description = description;
        SpeciesId = speciesId;
        BreedId = breedId;
        AgeMonths = ageMonths;
        Color = color;
        City = city;
        Vaccinated = vaccinated;
        Castrated = castrated;
        Status = ListingStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<AdoptionListing, ErrorList> Create(
        Guid userId,
        string userName,
        string userEmail,
        string? userPhone,
        string title,
        string description,
        Guid speciesId,
        Guid? breedId,
        int ageMonths,
        string color,
        string city,
        bool vaccinated,
        bool castrated)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(title))
            errors.Add(Error.Validation("listing.title_is_empty", "Назва оголошення не може бути порожньою"));
        else if (title.Length > MaxTitleLength)
            errors.Add(Error.Validation("listing.title_too_long", $"Назва оголошення занадто довга (максимум {MaxTitleLength} символів)"));

        if (string.IsNullOrWhiteSpace(description))
            errors.Add(Error.Validation("listing.description_is_empty", "Опис оголошення не може бути порожнім"));
        else if (description.Length > MaxDescriptionLength)
            errors.Add(Error.Validation("listing.description_too_long", $"Опис занадто довгий (максимум {MaxDescriptionLength} символів)"));

        if (speciesId == Guid.Empty)
            errors.Add(Error.Validation("listing.species_is_empty", "Вкажіть вид тварини"));

        if (ageMonths < 0)
            errors.Add(Error.Validation("listing.age_is_negative", "Вік не може бути від'ємним"));

        if (string.IsNullOrWhiteSpace(color))
            errors.Add(Error.Validation("listing.color_is_empty", "Вкажіть колір тварини"));

        if (string.IsNullOrWhiteSpace(city))
            errors.Add(Error.Validation("listing.city_is_empty", "Вкажіть місто"));

        if (errors.Count > 0)
            return new ErrorList(errors);

        return new AdoptionListing(
            Guid.NewGuid(), userId, userName, userEmail, userPhone,
            title, description, speciesId, breedId, ageMonths,
            color, city, vaccinated, castrated);
    }

    public UnitResult<Error> Update(
        string title,
        string description,
        Guid speciesId,
        Guid? breedId,
        int ageMonths,
        string color,
        string city,
        bool vaccinated,
        bool castrated,
        string? userPhone)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("listing.title_is_empty", "Назва оголошення не може бути порожньою");
        if (title.Length > MaxTitleLength)
            return Error.Validation("listing.title_too_long", "Назва занадто довга");
        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("listing.description_is_empty", "Опис не може бути порожнім");
        if (string.IsNullOrWhiteSpace(city))
            return Error.Validation("listing.city_is_empty", "Вкажіть місто");
        if (string.IsNullOrWhiteSpace(color))
            return Error.Validation("listing.color_is_empty", "Вкажіть колір");

        Title = title;
        Description = description;
        SpeciesId = speciesId;
        BreedId = breedId;
        AgeMonths = ageMonths;
        Color = color;
        City = city;
        Vaccinated = vaccinated;
        Castrated = castrated;
        UserPhone = userPhone;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddPhoto(string path)
    {
        if (Photos.Count >= MaxPhotos)
            return Error.Validation("listing.photos_limit_exceeded", $"Максимум {MaxPhotos} фото на оголошення");
        if (Photos.Contains(path))
            return Error.Conflict("listing.photo_already_exists", "Таке фото вже існує");
        Photos.Add(path);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemovePhoto(string path)
    {
        if (!Photos.Contains(path))
            return Error.NotFound("listing.photo_not_found", "Фото не знайдено");
        Photos.Remove(path);
        return UnitResult.Success<Error>();
    }

    public void MarkAdopted() => Status = ListingStatus.Adopted;

    public void Remove() => Status = ListingStatus.Removed;
}
