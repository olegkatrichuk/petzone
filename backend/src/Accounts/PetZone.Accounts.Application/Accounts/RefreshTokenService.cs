using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Contracts;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts;

public class RefreshTokenService(
    UserManager<User> userManager,
    IJwtTokenProvider jwtTokenProvider,
    IRefreshSessionRepository refreshSessionRepository,
    IAccountsUnitOfWork unitOfWork,
    IOptions<RefreshSessionOptions> refreshOptions,
    ILogger<RefreshTokenService> logger)
{
    public async Task<Result<LoginResult, ErrorList>> Handle(
        Guid refreshToken,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Refreshing token");

        var session = await refreshSessionRepository
            .GetByRefreshTokenAsync(refreshToken, cancellationToken);

        if (session is null)
            return (ErrorList)Error.NotFound("refresh.not_found", "Refresh token не найден.");

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            await refreshSessionRepository.DeleteAsync(session, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return (ErrorList)Error.Validation("refresh.expired", "Refresh token истёк.");
        }

        var user = session.User;
        var roles = await userManager.GetRolesAsync(user);

        // Удаляем старую сессию
        await refreshSessionRepository.DeleteAsync(session, cancellationToken);

        // Создаём новые токены
        var (accessToken, jti) = jwtTokenProvider.GenerateAccessToken(user, roles);
        var newRefreshToken = jwtTokenProvider.GenerateRefreshToken();

        var newSession = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = newRefreshToken,
            Jti = jti,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshOptions.Value.RefreshExpirationDays)
        };

        await refreshSessionRepository.AddAsync(newSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

        return new LoginResult(accessToken, refreshToken);
    }
}