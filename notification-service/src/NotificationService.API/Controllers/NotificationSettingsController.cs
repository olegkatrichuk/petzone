using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Commands.UpsertNotificationSettings;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationSettingsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertNotificationSettingsRequest request,
        [FromServices] UpsertNotificationSettingsHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpsertNotificationSettingsCommand(
            request.UserId,
            request.SendEmail,
            request.SendTelegram,
            request.SendWeb);

        var result = await handler.Handle(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

public record UpsertNotificationSettingsRequest(
    Guid UserId,
    bool? SendEmail,
    bool? SendTelegram,
    bool? SendWeb
);