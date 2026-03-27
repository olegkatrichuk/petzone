using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Framework.Files;

public interface IFilesProvider
{
    Task<Result<string, Error>> UploadFile(
        Stream stream,
        string bucketName,
        string fileName,
        CancellationToken ct = default);

    Task<Result<bool, Error>> DeleteFile(
        string bucketName,
        string fileName,
        CancellationToken ct = default);

    Task<Result<string, Error>> GetPresignedUrl(
        string bucketName,
        string fileName,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<FileInfo>, Error>> ListFiles(
        string bucketName,
        CancellationToken ct = default);

    public record FileInfo(string FileName, DateTime LastModified);
}