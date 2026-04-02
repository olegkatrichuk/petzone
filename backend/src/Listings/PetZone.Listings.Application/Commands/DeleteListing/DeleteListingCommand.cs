namespace PetZone.Listings.Application.Commands.DeleteListing;

public record DeleteListingCommand(Guid ListingId, Guid RequestingUserId);