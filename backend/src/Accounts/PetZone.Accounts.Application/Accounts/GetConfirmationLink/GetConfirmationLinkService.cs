using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.GetConfirmationLink;

public class GetConfirmationLinkService(UserManager<User> userManager)
{
    public async Task<Result<string, Error>> Handle(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Error.NotFound("user.not_found", $"User {userId} not found.");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return token;
    }
}