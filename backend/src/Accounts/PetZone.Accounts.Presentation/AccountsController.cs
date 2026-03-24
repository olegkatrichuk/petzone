using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Accounts;
using PetZone.Accounts.Application.Accounts.GetUserInfo;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Contracts;
using PetZone.SharedKernel;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Accounts.Presentation;

[ApiController]
[Route("[controller]")]
public class AccountsController(
    RegisterUserService registerUserService,
    LoginUserService loginUserService,
    RefreshTokenService refreshTokenService,
    ILogger<AccountsController> logger) : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";

    [HttpPost("register")]
    public async Task<ActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request);
        var result = await registerUserService.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request);
        var result = await loginUserService.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        SetRefreshTokenCookie(result.Value.RefreshToken);

        return this.ToOkResponse(new LoginResponse(result.Value.AccessToken));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshTokenStr)
            || !Guid.TryParse(refreshTokenStr, out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await refreshTokenService.Handle(refreshToken, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        SetRefreshTokenCookie(result.Value.RefreshToken);

        return this.ToOkResponse(new LoginResponse(result.Value.AccessToken));
    }
    
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserInfo(
        [FromRoute] Guid userId,
        [FromServices] GetUserInfoService service,
        CancellationToken cancellationToken)
    {
        var query = new GetUserInfoQuery(userId);
        var result = await service.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
    }

    private void SetRefreshTokenCookie(Guid refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookie, refreshToken.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}