namespace PetZone.Listings.Application.Commands.AddListingPhoto;

public record AddListingPhotoCommand(Guid ListingId, Guid UserId, string FileName);