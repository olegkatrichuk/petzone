namespace PetZone.Listings.Application.Commands.MarkAdopted;

public record MarkAdoptedCommand(Guid ListingId, Guid RequestingUserId);