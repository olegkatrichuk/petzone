namespace PetZone.Listings.Application.Queries;

public record GetAllListingsQuery(
    Guid? SpeciesId = null,
    string? City = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);