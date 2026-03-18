namespace PetZone.Domain.Shared;

public enum ErrorType
{
    Failure,    // Обычная ошибка бизнес-логики (по умолчанию)
    Validation, // Ошибка валидации данных (неверный email, пустое имя)
    NotFound,   // Сущность не найдена в БД
    Conflict    // Конфликт данных (например, уже существует)
}

public record Error(string Code, string Description, ErrorType Type, string? InvalidField = null)
{
    // Пустышка
    private static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    // Удобные методы для быстрого создания ошибок нужного типа
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description, string? invalidField = null) =>
        new(code, description, ErrorType.Validation, invalidField);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}