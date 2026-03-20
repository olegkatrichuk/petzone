using CSharpFunctionalExtensions;
using PetZone.Domain.Shared;

namespace PetZone.UseCases.Providers;

public interface IFilesProvider
{
    // Загрузить файл в хранилище
    Task<Result<string, Error>> UploadFile(
        Stream stream,
        string bucketName,
        string fileName,
        CancellationToken ct = default);

    // Удалить файл из хранилища
    Task<Result<bool, Error>> DeleteFile(
        string bucketName,
        string fileName,
        CancellationToken ct = default);

    // Получить presigned ссылку на файл
    Task<Result<string, Error>> GetPresignedUrl(
        string bucketName,
        string fileName,
        CancellationToken ct = default);
}