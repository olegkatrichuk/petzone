using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetZone.Accounts.Contracts.Dtos;
using PetZone.Accounts.Domain;
using PetZone.SharedKernel;

namespace PetZone.Accounts.Application.Accounts.GetUsers;

public class GetUsersService(UserManager<User> userManager)
{
    public async Task<Result<PagedResult<UserListItemDto>, Error>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = userManager.Users
            .Include(u => u.AdminAccount)
            .Include(u => u.VolunteerAccount)
            .Include(u => u.ParticipantAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(u =>
                u.Email!.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        var total = await q.CountAsync(cancellationToken);
        var users = await q
            .OrderBy(u => u.Email)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = users.Select(u => new UserListItemDto(
            Id: u.Id,
            Email: u.Email!,
            FirstName: u.FirstName,
            LastName: u.LastName,
            Role: u.AdminAccount != null ? "Admin"
                : u.VolunteerAccount != null ? "Volunteer"
                : "Participant",
            IsLocked: u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
            LockoutEnd: u.LockoutEnd
        )).ToList();

        return new PagedResult<UserListItemDto>(items, total);
    }
}

public record PagedResult<T>(List<T> Items, int TotalCount);
