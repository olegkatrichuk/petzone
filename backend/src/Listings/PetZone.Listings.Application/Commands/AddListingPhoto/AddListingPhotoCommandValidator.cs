using FluentValidation;

namespace PetZone.Listings.Application.Commands.AddListingPhoto;

public class AddListingPhotoCommandValidator : AbstractValidator<AddListingPhotoCommand>
{
    [Obsolete("Obsolete")]
    public AddListingPhotoCommandValidator()
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