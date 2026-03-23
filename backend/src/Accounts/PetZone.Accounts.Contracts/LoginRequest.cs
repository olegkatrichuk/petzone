namespace PetZone.Accounts.Contracts;

public record LoginRequest(
    string Email,
    string Password);