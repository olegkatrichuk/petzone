using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared; 

namespace PetZone.Domain.Models;

public class Weight : ValueObject
{
    // 1. ПУБЛИЧНЫЕ КОНСТАНТЫ
    public const double MIN_VALUE = 0;
    public const double MAX_VALUE = 2000; // На случай, если кто-то принесет лечить слона 🐘

    public double Value { get; }

    // 2. ПРИВАТНЫЙ КОНСТРУКТОР (никаких throw!)
    private Weight(double value)
    {
        Value = value;
    }

    private Weight() { } // Для EF Core

    // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
    public static Result<Weight, Error> Create(double value)
    {
        // Проверка на минимум
        if (value <= MIN_VALUE)
        {
            return Error.Validation("weight.too_small", "Вес должен быть больше нуля.");
        }

        // Защита от абсурдных данных
        if (value > MAX_VALUE)
        {
            return Error.Validation("weight.too_large", $"Вес не должен превышать {MAX_VALUE} кг.");
        }

        return new Weight(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}