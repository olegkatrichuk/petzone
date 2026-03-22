using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.SharedKernel;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Application.Providers;

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
    
    Task<Result<IReadOnlyList<FileInfo>, Error>> ListFiles(
        string bucketName,
        CancellationToken ct = default);

    public record FileInfo(string FileName, DateTime LastModified);
}