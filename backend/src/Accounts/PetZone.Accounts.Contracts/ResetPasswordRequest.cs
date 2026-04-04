namespace PetZone.Accounts.Contracts;

public record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);