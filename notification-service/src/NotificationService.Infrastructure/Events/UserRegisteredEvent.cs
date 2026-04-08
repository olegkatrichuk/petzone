namespace PetZone.Accounts.Application.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string ConfirmationToken
);
