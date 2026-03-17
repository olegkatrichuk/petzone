using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class Email : ValueObject
{
    // 1. ДОБАВЛЯЕМ ПУБЛИЧНУЮ КОНСТАНТУ 
    public const int MAX_LENGTH = 255;

    public string Value { get; } 

    private Email(string value)
    {
        Value = value;
    }

    private Email() { }

    public static Result<Email, Error> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("email.is_empty", "Email не может быть пустым.");
        }

        // 2. ИСПОЛЬЗУЕМ КОНСТАНТУ В ВАЛИДАЦИИ
        if (input.Length > MAX_LENGTH)
        {
            return Error.Validation("email.too_long", $"Email не должен превышать {MAX_LENGTH} символов.");
        }

        if (!input.Contains('@'))
        {
            return Error.Validation("email.is_invalid", "Некорректный формат Email.");
        }

        return new Email(input);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}