using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using PetZone.Domain.Shared;
using PetZone.UseCases.Providers;

namespace PetZone.Infrastructure.Providers;

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
            // Убеждаемся что bucket существует
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
                .WithExpiry(60 * 60 * 24); // 24 часа

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