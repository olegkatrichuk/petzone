using CSharpFunctionalExtensions;

namespace PetZone.Domain.Models;

public class Address : ValueObject
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("Город обязателен.");
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Улица обязательна.");
            
        City = city;
        Street = street;
    }

    private Address() { } // Для EF Core

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return City;
        yield return Street;
    }
}