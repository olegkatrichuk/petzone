using Microsoft.AspNetCore.Identity;
using PetZone.Accounts.Domain;
using PetZone.Core;

namespace PetZone.Accounts.Infrastructure;

public class UserInfoProvider(UserManager<User> userManager) : IUserInfoProvider
{
    public async Task<UserBasicInfo?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        return new UserBasicInfo(user.Email!, user.FirstName, user.LastName, user.PhoneNumber);
    }
}