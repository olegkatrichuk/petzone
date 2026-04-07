namespace PetZone.Listings.Contracts;

public record PagedListingsResult(IReadOnlyList<ListingDto> Items, int TotalCount, int Page, int PageSize);

public record ListingDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    string? UserPhone,
    string? ContactEmail,
    string Title,
    string Description,
    Guid SpeciesId,
    Guid? BreedId,
    int AgeMonths,
    string Color,
    string City,
    bool Vaccinated,
    bool Castrated,
    IReadOnlyList<string> Photos,
    string Status,
    DateTime CreatedAt
);