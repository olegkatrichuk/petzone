using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Commands.UpsertNotificationSettings;

namespace NotificationService.API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class NotificationSettingsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertNotificationSettingsRequest request,
        [FromServices] UpsertNotificationSettingsHandler handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var command = new UpsertNotificationSettingsCommand(
            userId,
            request.SendEmail,
            request.SendTelegram,
            request.SendWeb);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

public record UpsertNotificationSettingsRequest(
    bool? SendEmail,
    bool? SendTelegram,
    bool? SendWeb
);
