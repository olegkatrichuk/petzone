using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class Experience : ValueObject
{
    public int Years { get; } // Свойство должно быть публичным для сохранения в БД

    // 1. Приватный конструктор
    private Experience(int years)
    {
        Years = years;
    }

    private Experience() { } // Для EF Core

    // 2. Фабрика с валидацией
    public static Result<Experience, Error> Create(int years)
    {
        if (years < 0)
        {
            return Error.Validation("experience.is_negative", "Опыт не может быть отрицательным.");
        }

        return new Experience(years);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Years;
    }
}