using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

// Обязательно для нашего Error

namespace PetZone.Volunteers.Domain.Models;

public class Height : ValueObject
{
    // 1. ПУБЛИЧНЫЕ КОНСТАНТЫ
    public const double MIN_VALUE = 0;
    public const double MAX_VALUE = 500; // Допустим, 500 см — это максимум для любого питомца

    public double Value { get; }

    // 2. ПРИВАТНЫЙ КОНСТРУКТОР (никаких throw!)
    private Height(double value)
    {
        Value = value;
    }

    private Height() { } // Для EF Core

    // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
    public static Result<Height, Error> Create(double value)
    {
        // Проверка на минимум
        if (value <= MIN_VALUE)
        {
            return Error.Validation("height.too_small", "Рост должен быть больше нуля.");
        }

        // Защита от абсурдных данных (опечаток)
        if (value > MAX_VALUE)
        {
            return Error.Validation("height.too_large", $"Рост не должен превышать {MAX_VALUE}.");
        }

        return new Height(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}