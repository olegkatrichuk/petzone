namespace PetZone.SharedKernel;

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public record Error(string Code, string Description, ErrorType Type, string? InvalidField = null)
{
    private static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description, string? invalidField = null) =>
        new(code, description, ErrorType.Validation, invalidField);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);
}