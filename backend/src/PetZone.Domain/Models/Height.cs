using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models;

public class Height : ValueObject
{
    public double Value { get; }

    public Height(double value)
    {
        if (value <= 0) throw new ArgumentException("Рост должен быть больше 0.");
        Value = value;
    }

    private Height() { } // Для EF Core

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}