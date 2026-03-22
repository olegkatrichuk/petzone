using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

// Ваш класс Error

namespace PetZone.Volunteers.Domain.Models;

public class PhoneNumber : ValueObject
{
    // 1. ПУБЛИЧНАЯ КОНСТАНТА ДЛЯ ВАЛИДАЦИИ (Требование задания!)
    public const int MAX_LENGTH = 15;

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    private PhoneNumber() { } // Для EF Core

    // Ваш идеальный Result с кастомным Error
    public static Result<PhoneNumber, Error> Create(string input)
    {
        // Ручная валидация №1
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("phone.is_empty", "Номер телефона не может быть пустым.");
        }

        // Ручная валидация №2 (ИСПОЛЬЗУЕМ КОНСТАНТУ!)
        if (input.Length > MAX_LENGTH)
        {
            return Error.Validation("phone.too_long", $"Номер телефона не должен превышать {MAX_LENGTH} символов.");
        }

        return new PhoneNumber(input);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}