using PetZone.Accounts.Domain;

namespace PetZone.Accounts.Application;

public interface IJwtTokenProvider
{
    string GenerateToken(User user, IList<string> roles);
}