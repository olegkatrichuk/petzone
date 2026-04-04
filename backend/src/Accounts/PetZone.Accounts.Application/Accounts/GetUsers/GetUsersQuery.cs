namespace PetZone.Accounts.Application.Accounts.GetUsers;

public record GetUsersQuery(int Page, int PageSize, string? Search);