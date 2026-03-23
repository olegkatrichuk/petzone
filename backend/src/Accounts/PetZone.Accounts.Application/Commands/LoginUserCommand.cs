using PetZone.Accounts.Contracts;

namespace PetZone.Accounts.Application.Commands;

public record LoginUserCommand(LoginRequest Request);