using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models;

public class Weight : ValueObject
{
    public double Value { get; }

    public Weight(double value)
    {
        if (value <= 0) throw new ArgumentException("Вес должен быть больше 0.");
        Value = value;
    }

    private Weight() { } // Для EF Core

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}