using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class SocialNetwork : ValueObject
{
    public string Name { get; }
    public string Link { get; }

    // 1. Приватный конструктор
    private SocialNetwork(string name, string link)
    {
        Name = name;
        Link = link;
    }

    private SocialNetwork() { } // Для EF Core

    // 2. Фабрика с валидацией
    public static Result<SocialNetwork, Error> Create(string name, string link)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("socialnetwork.name_is_empty", "Название соц. сети обязательно.");
        }
        
        if (string.IsNullOrWhiteSpace(link))
        {
            return Error.Validation("socialnetwork.link_is_empty", "Ссылка обязательна.");
        }

        return new SocialNetwork(name, link);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Link;
    }
}