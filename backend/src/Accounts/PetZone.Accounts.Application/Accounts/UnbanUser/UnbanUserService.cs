using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.UnbanUser;

public class UnbanUserService(UserManager<User> userManager)
{
    public async Task<UnitResult<Error>> Handle(UnbanUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Error.NotFound("user.not_found", $"User {command.UserId} not found");

        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);

        return UnitResult.Success<Error>();
    }
}