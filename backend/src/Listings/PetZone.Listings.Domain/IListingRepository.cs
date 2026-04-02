namespace PetZone.Listings.Domain;

public interface IListingRepository
{
    Task<AdoptionListing?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AdoptionListing listing, CancellationToken ct = default);
    Task SaveAsync(AdoptionListing listing, CancellationToken ct = default);
    Task DeleteAsync(AdoptionListing listing, CancellationToken ct = default);
}