using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class Email : ValueObject
{
    // Сделал public, иначе мы не сможем прочитать сам email в сервисе!
    public string Value { get; } 

    // 1. Делаем основной конструктор ПРИВАТНЫМ. Теперь никаких throw!
    private Email(string value)
    {
        Value = value;
    }

    // 2. Оставляем пустой конструктор для EF Core (он молодец, пусть будет)
    private Email() { }

    // 3. Фабричный метод, который возвращает Result
    public static Result<Email, Error> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("email.is_empty", "Email не может быть пустым.");
        }

        if (!input.Contains('@'))
        {
            return Error.Validation("email.is_invalid", "Некорректный формат Email.");
        }

        // Если всё хорошо — возвращаем готовый объект
        return new Email(input);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}