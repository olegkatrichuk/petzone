namespace PetZone.Listings.Application.Queries;

public record GetMyListingsQuery(Guid UserId, int Page = 1, int PageSize = 20);