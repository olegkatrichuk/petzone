using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Contracts;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts;

public class LoginUserService(
    UserManager<User> userManager,
    IJwtTokenProvider jwtTokenProvider,
    IRefreshSessionRepository refreshSessionRepository,
    IAccountsUnitOfWork unitOfWork,
    IOptions<RefreshSessionOptions> refreshOptions,
    ILogger<LoginUserService> logger)
{
    public async Task<Result<LoginResult, ErrorList>> Handle(
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
        var (accessToken, jti) = jwtTokenProvider.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenProvider.GenerateRefreshToken();

        var refreshSession = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = refreshToken,
            Jti = jti,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshOptions.Value.RefreshExpirationDays)
        };

        await refreshSessionRepository.AddAsync(refreshSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {Email} logged in successfully", command.Request.Email);

        return new LoginResult(accessToken, refreshToken);
    }
}