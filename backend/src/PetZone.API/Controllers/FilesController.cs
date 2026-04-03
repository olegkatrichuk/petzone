using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [Authorize]
    [HttpPost("upload")]
    public async Task<ActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Uploading file {FileName}", file.FileName);

        await using var stream = file.OpenReadStream();

        var result = await filesProvider.UploadFile(
            stream,
            BucketName,
            file.FileName,
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
        logger.LogInformation("Deleting file {FileName}", fileName);

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
        var result = await filesProvider.GetPresignedUrl(BucketName, fileName, cancellationToken);
        if (result.IsFailure)
            return NotFound();
        return Redirect(result.Value);
    }
}