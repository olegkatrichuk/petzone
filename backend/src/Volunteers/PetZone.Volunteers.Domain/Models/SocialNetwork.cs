using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models;

public class SocialNetwork : ValueObject
{
    // 1. ПУБЛИЧНЫЕ КОНСТАНТЫ
    public const int MAX_NAME_LENGTH = 50;
    public const int MAX_LINK_LENGTH = 2000; // Ссылки бывают очень длинными

    public string Name { get; }
    public string Link { get; }

    private SocialNetwork(string name, string link)
    {
        Name = name;
        Link = link;
    }

    private SocialNetwork() { } // Для EF Core

    public static Result<SocialNetwork, Error> Create(string name, string link)
    {
        // --- Валидация названия ---
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("socialnetwork.name_is_empty", "Название соц. сети обязательно.");
            
        if (name.Length > MAX_NAME_LENGTH)
            return Error.Validation("socialnetwork.name_too_long", $"Название не должно превышать {MAX_NAME_LENGTH} символов.");
        
        // --- Валидация ссылки ---
        if (string.IsNullOrWhiteSpace(link))
            return Error.Validation("socialnetwork.link_is_empty", "Ссылка обязательна.");

        if (link.Length > MAX_LINK_LENGTH)
            return Error.Validation("socialnetwork.link_too_long", $"Ссылка не должна превышать {MAX_LINK_LENGTH} символов.");

        // Создаем объект, очищая строки от случайных пробелов
        return new SocialNetwork(name.Trim(), link.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Link;
    }
}