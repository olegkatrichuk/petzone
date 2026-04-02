namespace PetZone.Listings.Application.Commands.RemoveListingPhoto;

public record RemoveListingPhotoCommand(Guid ListingId, Guid UserId, string FileName);