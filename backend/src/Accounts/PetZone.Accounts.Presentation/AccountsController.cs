using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Accounts;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Contracts;
using PetZone.Volunteers.Presentation;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Accounts.Presentation;

[ApiController]
[Route("[controller]")]
public class AccountsController(
    RegisterUserService registerUserService,
    LoginUserService loginUserService,
    ILogger<AccountsController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering user {Email}", request.Email);

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
        logger.LogInformation("Login attempt for {Email}", request.Email);

        var command = new LoginUserCommand(request);
        var result = await loginUserService.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return this.ToOkResponse(result.Value);
    }
}