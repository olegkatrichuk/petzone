using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class PhoneNumber : ValueObject
{
    // Сделали public, чтобы EF Core мог сохранять номер в базу
    public string Value { get; }

    // 1. Прячем конструктор и убираем throw
    private PhoneNumber(string value)
    {
        Value = value;
    }

    private PhoneNumber() { } // Для EF Core

    // 2. Добавляем нашу фабрику Create
    public static Result<PhoneNumber, Error> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("phone.is_empty", "Номер телефона не может быть пустым.");
        }

        return new PhoneNumber(input);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}