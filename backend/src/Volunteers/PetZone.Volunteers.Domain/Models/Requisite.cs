using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models;

public class Requisite : ValueObject
{
    // 1. ПУБЛИЧНЫЕ КОНСТАНТЫ
    public const int MAX_NAME_LENGTH = 100;
    public const int MAX_DESCRIPTION_LENGTH = 500;

    public string Name { get; }
    public string Description { get; }

    private Requisite(string name, string description)
    {
        Name = name;
        Description = description;
    }

    private Requisite() { } // Для EF Core

    public static Result<Requisite, Error> Create(string name, string description)
    {
        // --- Валидация названия ---
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("requisite.name_is_empty", "Название реквизита обязательно.");
            
        if (name.Length > MAX_NAME_LENGTH)
            return Error.Validation("requisite.name_too_long", $"Название реквизита не должно превышать {MAX_NAME_LENGTH} символов.");

        // --- Валидация описания ---
        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("requisite.description_is_empty", "Описание реквизита обязательно.");
            
        if (description.Length > MAX_DESCRIPTION_LENGTH)
            return Error.Validation("requisite.description_too_long", $"Описание реквизита не должно превышать {MAX_DESCRIPTION_LENGTH} символов.");
            
        // Создаем объект, очищая строки от случайных пробелов
        return new Requisite(name.Trim(), description.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Description;
    }
}