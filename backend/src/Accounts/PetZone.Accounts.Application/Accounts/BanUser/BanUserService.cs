using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.BanUser;

public class BanUserService(UserManager<User> userManager)
{
    public async Task<UnitResult<Error>> Handle(BanUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Error.NotFound("user.not_found", $"User {command.UserId} not found");

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return UnitResult.Success<Error>();
    }
}