namespace PetZone.Accounts.Contracts.Dtos;

public record UserListItemDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsLocked,
    DateTimeOffset? LockoutEnd
);
