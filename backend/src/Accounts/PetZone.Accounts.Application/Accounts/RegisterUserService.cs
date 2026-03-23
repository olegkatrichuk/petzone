using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts;

public class RegisterUserService(
    UserManager<User> userManager,
    ILogger<RegisterUserService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Registering user with email {Email}", command.Request.Email);

        var existingUser = await userManager.FindByEmailAsync(command.Request.Email);
        if (existingUser is not null)
            return (ErrorList)Error.Conflict("user.already_exists", "Пользователь с таким email уже существует.");

        var user = new User
        {
            UserName = command.Request.Email,
            Email = command.Request.Email,
            FirstName = command.Request.FirstName,
            LastName = command.Request.LastName
        };

        var result = await userManager.CreateAsync(user, command.Request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
            return new ErrorList(errors);
        }

        await userManager.AddToRoleAsync(user, Role.Participant);

        logger.LogInformation("User {Email} registered successfully", command.Request.Email);

        return user.Id;
    }
}