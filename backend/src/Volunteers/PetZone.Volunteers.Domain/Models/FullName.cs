using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models;

public class FullName : ValueObject
{
    // 1. ДОБАВЛЯЕМ ПУБЛИЧНЫЕ КОНСТАНТЫ
    public const int MAX_FIRST_NAME_LENGTH = 50;
    public const int MAX_LAST_NAME_LENGTH = 50;
    public const int MAX_PATRONYMIC_LENGTH = 50;

    public string FirstName { get; }
    public string LastName { get; }
    public string Patronymic { get; }

    private FullName(string firstName, string lastName, string patronymic)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
    }

    private FullName() { } 

    public static Result<FullName, Error> Create(string firstName, string lastName, string patronymic = "")
    {
        // --- Проверка Имени ---
        if (string.IsNullOrWhiteSpace(firstName))
            return Error.Validation("fullname.firstname_is_empty", "Имя обязательно.");
            
        if (firstName.Length > MAX_FIRST_NAME_LENGTH)
            return Error.Validation("fullname.firstname_too_long", $"Имя не должно превышать {MAX_FIRST_NAME_LENGTH} символов.");

        // --- Проверка Фамилии ---
        if (string.IsNullOrWhiteSpace(lastName))
            return Error.Validation("fullname.lastname_is_empty", "Фамилия обязательна.");
            
        if (lastName.Length > MAX_LAST_NAME_LENGTH)
            return Error.Validation("fullname.lastname_too_long", $"Фамилия не должна превышать {MAX_LAST_NAME_LENGTH} символов.");

        // --- Проверка Отчества (только если оно передано) ---
        if (!string.IsNullOrWhiteSpace(patronymic) && patronymic.Length > MAX_PATRONYMIC_LENGTH)
            return Error.Validation("fullname.patronymic_too_long", $"Отчество не должно превышать {MAX_PATRONYMIC_LENGTH} символов.");

        // Создаем объект, попутно очищая строки от лишних пробелов по краям
        return new FullName(
            firstName.Trim(), 
            lastName.Trim(), 
            string.IsNullOrWhiteSpace(patronymic) ? string.Empty : patronymic.Trim()
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Patronymic;
    }
}