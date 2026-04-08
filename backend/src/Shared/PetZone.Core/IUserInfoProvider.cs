namespace PetZone.Core;

public record UserBasicInfo(string Email, string FirstName, string LastName);

public interface IUserInfoProvider
{
    Task<UserBasicInfo?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}