using System.Collections.Generic;
using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.Domain.Models;

public class PetPhoto : ValueObject
{
    public const int MAX_PATH_LENGTH = 500;

    public string FilePath { get; }
    public bool IsMain { get; }

    private PetPhoto(string filePath, bool isMain)
    {
        FilePath = filePath;
        IsMain = isMain;
    }

    private PetPhoto() { }

    public static Result<PetPhoto, Error> Create(string filePath, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Error.Validation("petphoto.path_is_empty", "Путь к фото не может быть пустым.");

        if (filePath.Length > MAX_PATH_LENGTH)
            return Error.Validation("petphoto.path_too_long",
                $"Путь не должен превышать {MAX_PATH_LENGTH} символов.");

        return new PetPhoto(filePath.Trim(), isMain);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FilePath;
    }
}