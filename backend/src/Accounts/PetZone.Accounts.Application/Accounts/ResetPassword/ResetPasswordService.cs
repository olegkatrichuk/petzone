using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Events;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.ResetPassword;

public class ResetPasswordService(
    UserManager<User> userManager,
    IPublisher publisher,
    ILogger<ResetPasswordService> logger)
{
    public async Task<Result<bool, Error>> Handle(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Error.NotFound("user.not_found", $"User {userId} not found.");

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Error.Validation("password.reset_failed",
                string.Join(", ", result.Errors.Select(e => e.Description)));

        await publisher.Publish(new UserCacheInvalidationEvent(userId), cancellationToken);

        logger.LogInformation("Password reset successfully for user {UserId}", userId);
        return true;
    }
}