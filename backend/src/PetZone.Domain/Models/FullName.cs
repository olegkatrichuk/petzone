using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared; // Не забываем про наши ошибки

namespace PetZone.Domain.Models;

public class FullName : ValueObject
{
    // Сделали свойства публичными для чтения, иначе в базу не сохранятся!
    public string FirstName { get; }
    public string LastName { get; }
    public string Patronymic { get; }

    // 1. Конструктор теперь ПРИВАТНЫЙ и без throw
    private FullName(string firstName, string lastName, string patronymic)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
    }

    private FullName() { } // Для EF Core

    // 2. Наша фабрика с Result
    public static Result<FullName, Error> Create(string firstName, string lastName, string patronymic = "")
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Error.Validation("fullname.firstname_is_empty", "Имя обязательно.");
        }
        
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Error.Validation("fullname.lastname_is_empty", "Фамилия обязательна.");
        }

        return new FullName(firstName, lastName, patronymic);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Patronymic;
    }
}