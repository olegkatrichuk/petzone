namespace PetZone.Listings.Domain;

public interface IListingRepository
{
    Task<AdoptionListing?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AdoptionListing listing, CancellationToken ct = default);
    void Save(AdoptionListing listing);
    void Delete(AdoptionListing listing);
    Task<bool> ActiveListingExistsAsync(Guid userId, string title, CancellationToken ct = default);
}