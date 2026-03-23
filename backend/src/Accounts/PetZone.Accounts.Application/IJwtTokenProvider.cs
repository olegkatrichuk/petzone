using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Application;

public interface IJwtTokenProvider
{
    (string AccessToken, Guid Jti) GenerateAccessToken(User user, IList<string> roles);
    Guid GenerateRefreshToken();
}