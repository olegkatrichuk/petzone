using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class Requisite : ValueObject
{
    // Свойства делаем public для базы данных
    public string Name { get; }
    public string Description { get; }

    // 1. Прячем конструктор и убираем throw
    private Requisite(string name, string description)
    {
        Name = name;
        Description = description;
    }

    private Requisite() { } // Для EF Core

    // 2. Фабрика с Result
    public static Result<Requisite, Error> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("requisite.name_is_empty", "Название реквизита обязательно.");
            
        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("requisite.description_is_empty", "Описание реквизита обязательно.");
            
        return new Requisite(name, description);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Description;
    }
}