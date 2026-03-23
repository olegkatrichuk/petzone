using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Contracts;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts;

public class LoginUserService(
    UserManager<User> userManager,
    IJwtTokenProvider jwtTokenProvider,
    ILogger<LoginUserService> logger)
{
    public async Task<Result<LoginResponse, ErrorList>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Login attempt for {Email}", command.Request.Email);

        var user = await userManager.FindByEmailAsync(command.Request.Email);
        if (user is null)
            return (ErrorList)Error.NotFound("user.not_found", "Неверный email или пароль.");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, command.Request.Password);
        if (!isPasswordValid)
            return (ErrorList)Error.Validation("user.invalid_password", "Неверный email или пароль.");

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenProvider.GenerateToken(user, roles);

        logger.LogInformation("User {Email} logged in successfully", command.Request.Email);

        return new LoginResponse(token);
    }
}