using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Application.Commands;
using PetZone.Accounts.Application.Repositories;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts;

public class RegisterUserService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IParticipantAccountRepository participantAccountRepository,
    IAccountsUnitOfWork unitOfWork,
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

        var participantRole = await roleManager.FindByNameAsync(Role.Participant);
        if (participantRole is null)
            return (ErrorList)Error.NotFound("role.not_found", "Роль Participant не найдена.");

        var user = User.CreateParticipant(
            command.Request.Email,
            command.Request.FirstName,
            command.Request.LastName,
            participantRole);

        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await userManager.CreateAsync(user, command.Request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => Error.Validation(e.Code, e.Description))
                    .ToList();
                return new ErrorList(errors);
            }

            await userManager.AddToRoleAsync(user, Role.Participant);

            var participantAccount = new ParticipantAccount
            {
                Id = Guid.NewGuid(),
                UserId = user.Id
            };

            await participantAccountRepository.AddAsync(participantAccount, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User {Email} registered successfully", command.Request.Email);
            return user.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register user {Email}", command.Request.Email);
            return (ErrorList)Error.Failure("user.register_failed", "Ошибка при регистрации пользователя.");
        }
    }
}