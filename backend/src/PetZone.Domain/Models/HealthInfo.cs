using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class HealthInfo : ValueObject
{
    // 1. ПУБЛИЧНЫЕ КОНСТАНТЫ
    public const int MAX_GENERAL_DESCRIPTION_LENGTH = 2000;
    public const int MAX_DIET_OR_ALLERGIES_LENGTH = 1000;

    public string GeneralDescription { get; }
    public string DietOrAllergies { get; }

    // 2. ПРИВАТНЫЙ КОНСТРУКТОР (без throw!)
    private HealthInfo(string generalDescription, string dietOrAllergies)
    {
        GeneralDescription = generalDescription;
        DietOrAllergies = dietOrAllergies;
    }

    private HealthInfo() { } // Для EF Core

    // 3. ФАБРИЧНЫЙ МЕТОД С РУЧНОЙ ВАЛИДАЦИЕЙ
    public static Result<HealthInfo, Error> Create(string generalDescription, string dietOrAllergies = "")
    {
        // --- Валидация общего описания ---
        if (string.IsNullOrWhiteSpace(generalDescription))
        {
            return Error.Validation("healthinfo.description_is_empty", "Описание здоровья обязательно.");
        }

        if (generalDescription.Length > MAX_GENERAL_DESCRIPTION_LENGTH)
        {
            return Error.Validation("healthinfo.description_too_long", $"Описание здоровья не должно превышать {MAX_GENERAL_DESCRIPTION_LENGTH} символов.");
        }

        // --- Валидация диет и аллергий (если они указаны) ---
        if (!string.IsNullOrWhiteSpace(dietOrAllergies) && dietOrAllergies.Length > MAX_DIET_OR_ALLERGIES_LENGTH)
        {
            return Error.Validation("healthinfo.diet_too_long", $"Описание диеты или аллергий не должно превышать {MAX_DIET_OR_ALLERGIES_LENGTH} символов.");
        }

        // Возвращаем объект, очищая строки от случайных пробелов
        return new HealthInfo(
            generalDescription.Trim(), 
            string.IsNullOrWhiteSpace(dietOrAllergies) ? string.Empty : dietOrAllergies.Trim()
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return GeneralDescription;
        yield return DietOrAllergies;
    }
}