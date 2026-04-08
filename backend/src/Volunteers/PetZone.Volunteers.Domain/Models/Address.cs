using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

// Не забываем про ваш класс Error!

namespace PetZone.Volunteers.Domain.Models;

public class Address : ValueObject
{
    // 1. ПУБЛИЧНЫЕ КОНСТАНТЫ ДЛЯ ВАЛИДАЦИИ (Для EF Core и FluentValidation)
    public const int MAX_CITY_LENGTH = 50;
    public const int MAX_STREET_LENGTH = 100;

    // Свойства только для чтения
    public string City { get; }
    public string? Street { get; }

    // 2. ПРИВАТНЫЙ КОНСТРУКТОР (без всяких throw!)
    private Address(string city, string? street)
    {
        City = city;
        Street = street;
    }

    private Address() { } // Для EF Core

    // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
    public static Result<Address, Error> Create(string city, string? street = null)
    {
        // --- ВАЛИДАЦИЯ ГОРОДА ---
        if (string.IsNullOrWhiteSpace(city))
        {
            return Error.Validation("address.city_is_empty", "Название города не может быть пустым.");
        }

        if (city.Length > MAX_CITY_LENGTH)
        {
            return Error.Validation("address.city_too_long", $"Название города не должно превышать {MAX_CITY_LENGTH} символов.");
        }

        // --- ВАЛИДАЦИЯ УЛИЦЫ (необязательная) ---
        if (!string.IsNullOrWhiteSpace(street) && street.Length > MAX_STREET_LENGTH)
        {
            return Error.Validation("address.street_too_long", $"Название улицы не должно превышать {MAX_STREET_LENGTH} символов.");
        }

        return new Address(city, string.IsNullOrWhiteSpace(street) ? null : street);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return City;
        yield return Street;
    }
}