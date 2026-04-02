namespace PetZone.Listings.Application.Events;

public record ListingAdoptedEvent(
    Guid ListingId,
    string UserEmail,
    string UserName,
    string Title
);