using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class Experience : ValueObject
{
    // 1. ДОБАВЛЯЕМ ПУБЛИЧНЫЕ КОНСТАНТЫ ДЛЯ ЛИМИТОВ
    public const int MIN_YEARS = 0;
    public const int MAX_YEARS = 100; // Разумный предел, чтобы отсечь опечатки

    public int Years { get; }

    private Experience(int years)
    {
        Years = years;
    }

    private Experience() { } // Для EF Core

    public static Result<Experience, Error> Create(int years)
    {
        // 2. ИСПОЛЬЗУЕМ КОНСТАНТЫ В ВАЛИДАЦИИ
        if (years < MIN_YEARS)
        {
            return Error.Validation("experience.too_small", $"Опыт не может быть меньше {MIN_YEARS} лет.");
        }

        // Защита от абсурдных данных
        if (years > MAX_YEARS)
        {
            return Error.Validation("experience.too_large", $"Опыт не может превышать {MAX_YEARS} лет.");
        }

        return new Experience(years);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Years;
    }
}