using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.ConfirmEmail;

public class ConfirmEmailService(UserManager<User> userManager)
{
    public async Task<Result<bool, Error>> Handle(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Error.NotFound("user.not_found", $"User {userId} not found.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Error.Validation("email.confirmation_failed",
                string.Join(", ", result.Errors.Select(e => e.Description)));

        return true;
    }
}