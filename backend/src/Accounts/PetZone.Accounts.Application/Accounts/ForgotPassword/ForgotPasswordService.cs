using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Events;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.ForgotPassword;

public class ForgotPasswordService(
    UserManager<User> userManager,
    IPublishEndpoint publishEndpoint,
    ILogger<ForgotPasswordService> logger)
{
    public async Task<Result<bool, Error>> Handle(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        // Always return success to avoid user enumeration
        if (user is null)
        {
            logger.LogInformation("ForgotPassword requested for unknown email {Email}", email);
            return true;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        await publishEndpoint.Publish(new ForgotPasswordEvent(
            user.Id,
            user.Email!,
            user.FirstName,
            token), cancellationToken);

        return true;
    }
}