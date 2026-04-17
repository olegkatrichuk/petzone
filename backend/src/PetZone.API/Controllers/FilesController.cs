using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetZone.API.Extensions;
using PetZone.Framework.Files;

namespace PetZone.API.Controllers;

[ApiController]
[Route("[controller]")]
public class FilesController(
    IFilesProvider filesProvider,
    ILogger<FilesController> logger) : ControllerBase
{
    private const string BucketName = "petzone";
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif"
    ];

    // Only safe filename characters: letters, digits, hyphens, dots, underscores
    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9_\-\.]+$", RegexOptions.Compiled);

    [Authorize]
    [EnableRateLimiting("file-upload")]
    [HttpPost("upload")]
    public async Task<ActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("File is empty.");

        if (file.Length > MaxFileSizeBytes)
            return BadRequest($"File exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB.");

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("File type not allowed. Only JPEG, PNG, WebP, and GIF images are accepted.");

        // Generate a UUID-based name to prevent path traversal and name collisions
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeFileName = $"{Guid.NewGuid()}{extension}";

        logger.LogInformation("Uploading file {OriginalFileName} as {SafeFileName} by user {UserId}",
            file.FileName, safeFileName, User.FindFirst("sub")?.Value);

        await using var stream = file.OpenReadStream();

        var result = await filesProvider.UploadFile(
            stream,
            BucketName,
            safeFileName,
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [Authorize]
    [HttpDelete("{fileName}")]
    public async Task<ActionResult> Delete(
        [FromRoute] string fileName,
        CancellationToken cancellationToken)
    {
        if (!SafeFileNameRegex.IsMatch(fileName))
            return BadRequest("Invalid file name.");

        logger.LogInformation("Deleting file {FileName} by user {UserId}",
            fileName, User.FindFirst("sub")?.Value);

        var result = await filesProvider.DeleteFile(
            BucketName,
            fileName,
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [HttpGet("{fileName}/url")]
    public async Task<ActionResult> GetPresignedUrl(
        [FromRoute] string fileName,
        CancellationToken cancellationToken)
    {
        if (!SafeFileNameRegex.IsMatch(fileName))
            return BadRequest("Invalid file name.");

        logger.LogInformation("Getting presigned URL for {FileName}", fileName);

        var result = await filesProvider.GetPresignedUrl(
            BucketName,
            fileName,
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [HttpGet("{fileName}/redirect")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult> RedirectToFile(
        [FromRoute] string fileName,
        CancellationToken cancellationToken)
    {
        if (!SafeFileNameRegex.IsMatch(fileName))
            return BadRequest("Invalid file name.");

        var result = await filesProvider.GetPresignedUrl(BucketName, fileName, cancellationToken);
        if (result.IsFailure)
            return NotFound();
        return Redirect(result.Value);
    }
}