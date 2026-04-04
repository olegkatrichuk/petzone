namespace PetZone.Accounts.Application.Events;

public record ForgotPasswordEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string ResetToken
);