using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetZone.Accounts.Application.Accounts.BanUser;
using PetZone.Accounts.Application.Accounts.GetUsers;
using PetZone.Accounts.Application.Accounts.UnbanUser;
using PetZone.Accounts.Infrastructure.Authorization;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Accounts.Presentation;

[ApiController]
[Route("admin")]
[Authorize]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Permissions.Users.Read)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromServices] GetUsersService service = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(page, pageSize, search);
        var result = await service.Handle(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [HttpPost("users/{userId:guid}/ban")]
    [Authorize(Permissions.Users.Ban)]
    public async Task<IActionResult> BanUser(
        [FromRoute] Guid userId,
        [FromServices] BanUserService service,
        CancellationToken cancellationToken)
    {
        var result = await service.Handle(new BanUserCommand(userId), cancellationToken);
        return result.IsSuccess ? Ok() : result.Error.ToResponse();
    }

    [HttpPost("users/{userId:guid}/unban")]
    [Authorize(Permissions.Users.Ban)]
    public async Task<IActionResult> UnbanUser(
        [FromRoute] Guid userId,
        [FromServices] UnbanUserService service,
        CancellationToken cancellationToken)
    {
        var result = await service.Handle(new UnbanUserCommand(userId), cancellationToken);
        return result.IsSuccess ? Ok() : result.Error.ToResponse();
    }
}
