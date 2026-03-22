namespace PetZone.Volunteers.Application.Queries;

public record GetPetsQuery(
    // Пагинация
    int Page,
    int PageSize,

    // Фильтрация
    Guid? VolunteerId,
    string? Nickname,
    string? Color,
    string? City,
    Guid? SpeciesId,
    Guid? BreedId,
    int? MinAge,
    int? MaxAge,
    double? MinWeight,
    double? MaxWeight,
    bool? IsCastrated,
    bool? IsVaccinated,
    int? Status,

    // Сортировка
    string? SortBy,
    bool SortDescending = false);