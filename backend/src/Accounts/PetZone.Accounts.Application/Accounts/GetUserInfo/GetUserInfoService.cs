using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.Accounts.Contracts.Dtos;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.GetUserInfo;

public class GetUserInfoService(
    UserManager<User> userManager,
    ILogger<GetUserInfoService> logger)
{
    public async Task<Result<UserDto, Error>> Handle(
        GetUserInfoQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user info for {UserId}", query.UserId);

        var user = await userManager.Users
            .Include(u => u.ParticipantAccount)
            .Include(u => u.VolunteerAccount)
            .Include(u => u.AdminAccount)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
            return Error.NotFound("user.not_found", $"User with id {query.UserId} not found.");

        var dto = new UserDto(
            Id: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            ParticipantAccount: user.ParticipantAccount is null ? null : new ParticipantAccountDto(
                user.ParticipantAccount.Id,
                user.ParticipantAccount.FavoritePets
            ),
            VolunteerAccount: user.VolunteerAccount is null ? null : new VolunteerAccountDto(
                user.VolunteerAccount.Id,
                user.VolunteerAccount.Experience,
                user.VolunteerAccount.Certificates,
                user.VolunteerAccount.Requisites
            ),
            AdminAccount: user.AdminAccount is null ? null : new AdminAccountDto(
                user.AdminAccount.Id
            )
        );

        return dto;
    }
}