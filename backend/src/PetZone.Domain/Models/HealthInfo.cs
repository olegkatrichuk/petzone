using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models;

public class HealthInfo : ValueObject
{
    public string GeneralDescription { get; }
    public string DietOrAllergies { get; }

    public HealthInfo(string generalDescription, string dietOrAllergies = "")
    {
        if (string.IsNullOrWhiteSpace(generalDescription)) throw new ArgumentException("Описание здоровья обязательно.");
        GeneralDescription = generalDescription;
        DietOrAllergies = dietOrAllergies;
    }

    private HealthInfo() { } // Для EF Core

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return GeneralDescription;
        yield return DietOrAllergies;
    }
}