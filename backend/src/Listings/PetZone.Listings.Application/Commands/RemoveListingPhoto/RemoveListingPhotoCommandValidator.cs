using FluentValidation;

namespace PetZone.Listings.Application.Commands.RemoveListingPhoto;

public class RemoveListingPhotoCommandValidator : AbstractValidator<RemoveListingPhotoCommand>
{
    [Obsolete("Obsolete")]
    public RemoveListingPhotoCommandValidator()
    {
        RuleFor(c => c.ListingId)
            .NotEmpty()
                .WithErrorCode("listing.id_is_empty")
                .WithMessage("Id оголошення обов'язковий");

        RuleFor(c => c.FileName)
            .NotEmpty()
                .WithErrorCode("listing.filename_is_empty")
                .WithMessage("Ім'я файлу не може бути порожнім");
    }
}