namespace PetZone.Accounts.Application.Accounts;

public record LoginResult(string AccessToken, Guid RefreshToken);