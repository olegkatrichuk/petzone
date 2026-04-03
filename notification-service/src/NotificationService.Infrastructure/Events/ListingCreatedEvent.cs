namespace PetZone.Listings.Application.Events;

public record ListingCreatedEvent(
    Guid ListingId,
    string UserEmail,
    string UserName,
    string Title,
    string City
);