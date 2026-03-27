using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using PetZone.SharedKernel;

namespace PetZone.Framework.Files;

public class MinioProvider(
    IMinioClient minioClient,
    ILogger<MinioProvider> logger) : IFilesProvider
{
    public async Task<Result<string, Error>> UploadFile(
        Stream stream,
        string bucketName,
        string fileName,
        CancellationToken ct = default)
    {
        try
        {
            await EnsureBucketExists(bucketName, ct);

            var putArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/octet-stream");

            await minioClient.PutObjectAsync(putArgs, ct);

            logger.LogInformation("File {FileName} uploaded to bucket {BucketName}", fileName, bucketName);

            return fileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload file {FileName} to bucket {BucketName}", fileName, bucketName);
            return Error.Failure("minio.upload_failed", $"Не удалось загрузить файл {fileName}.");
        }
    }

    public async Task<Result<bool, Error>> DeleteFile(
        string bucketName,
        string fileName,
        CancellationToken ct = default)
    {
        try
        {
            var removeArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName);

            await minioClient.RemoveObjectAsync(removeArgs, ct);

            logger.LogInformation("File {FileName} deleted from bucket {BucketName}", fileName, bucketName);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file {FileName} from bucket {BucketName}", fileName, bucketName);
            return Error.Failure("minio.delete_failed", $"Не удалось удалить файл {fileName}.");
        }
    }

    public async Task<Result<string, Error>> GetPresignedUrl(
        string bucketName,
        string fileName,
        CancellationToken ct = default)
    {
        try
        {
            var presignedArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithExpiry(60 * 60 * 24);

            var url = await minioClient.PresignedGetObjectAsync(presignedArgs);

            logger.LogInformation("Presigned URL generated for {FileName}", fileName);

            return url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get presigned URL for {FileName}", fileName, bucketName);
            return Error.Failure("minio.presigned_failed", $"Не удалось получить ссылку на файл {fileName}.");
        }
    }

    public async Task<Result<IReadOnlyList<IFilesProvider.FileInfo>, Error>> ListFiles(
        string bucketName,
        CancellationToken ct = default)
    {
        try
        {
            await EnsureBucketExists(bucketName, ct);

            var files = new List<IFilesProvider.FileInfo>();

            var listArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithRecursive(true);

            await foreach (var item in minioClient.ListObjectsEnumAsync(listArgs, ct))
            {
                files.Add(new IFilesProvider.FileInfo(
                    item.Key,
                    item.LastModifiedDateTime?.ToUniversalTime() ?? DateTime.UtcNow));
            }

            logger.LogInformation("Listed {Count} files in bucket {BucketName}", files.Count, bucketName);

            return files.AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list files in bucket {BucketName}", bucketName);
            return Error.Failure("minio.list_failed", "Не удалось получить список файлов.");
        }
    }

    private async Task EnsureBucketExists(string bucketName, CancellationToken ct)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await minioClient.BucketExistsAsync(bucketExistsArgs, ct);

        if (!exists)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
            await minioClient.MakeBucketAsync(makeBucketArgs, ct);
            logger.LogInformation("Bucket {BucketName} created", bucketName);
        }
    }
}