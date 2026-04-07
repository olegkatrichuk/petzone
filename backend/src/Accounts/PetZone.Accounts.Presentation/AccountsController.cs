using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Accounts;
using PetZone.Accounts.Application.Accounts.ConfirmEmail;
using PetZone.Accounts.Application.Accounts.ForgotPassword;
using PetZone.Accounts.Application.Accounts.GetConfirmationLink;
using PetZone.Accounts.Application.Accounts.GetUserInfo;
using PetZone.Accounts.Application.Accounts.ResetPassword;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Application;
using PetZone.Accounts.Application.Repositories;
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
    IRefreshSessionRepository refreshSessionRepository,
    IAccountsUnitOfWork unitOfWork,
    ILogger<AccountsController> logger) : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";

    [EnableRateLimiting("auth")]
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

    [EnableRateLimiting("auth")]
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

    [EnableRateLimiting("auth")]
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
    
    [Authorize]
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
    
    // GET /accounts/{userId}/confirmation-token
    [Authorize]
    [HttpGet("{userId:guid}/confirmation-token")]
    public async Task<IActionResult> GetConfirmationToken(
        [FromRoute] Guid userId,
        [FromServices] GetConfirmationLinkService service,
        CancellationToken cancellationToken)
    {
        var result = await service.Handle(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    

// GET /accounts/confirm-email
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] Guid userId,
        [FromQuery] string token,
        [FromServices] ConfirmEmailService service,
        CancellationToken cancellationToken)
    {
        var result = await service.Handle(userId, token, cancellationToken);
        return result.IsSuccess ? Ok("Email confirmed successfully") : BadRequest(result.Error);
    }

    [EnableRateLimiting("forgot-password")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] ForgotPasswordService service,
        CancellationToken cancellationToken)
    {
        await service.Handle(request.Email, cancellationToken);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] ResetPasswordService service,
        CancellationToken cancellationToken)
    {
        var result = await service.Handle(request.UserId, request.Token, request.NewPassword, cancellationToken);
        return result.IsSuccess ? Ok() : result.Error.ToResponse();
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshTokenStr)
            || !Guid.TryParse(refreshTokenStr, out var refreshToken))
        {
            Response.Cookies.Delete(RefreshTokenCookie);
            return Ok();
        }

        var session = await refreshSessionRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (session is not null)
        {
            await refreshSessionRepository.DeleteAsync(session, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        Response.Cookies.Delete(RefreshTokenCookie);
        logger.LogInformation("User logged out");
        return Ok();
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