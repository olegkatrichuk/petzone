using PetZone.Accounts.Contracts;

namespace PetZone.Accounts.Application.Commands;

public record RegisterUserCommand(RegisterRequest Request);