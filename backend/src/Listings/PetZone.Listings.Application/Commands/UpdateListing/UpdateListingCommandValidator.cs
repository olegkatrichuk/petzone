using FluentValidation;
using PetZone.Listings.Domain;

namespace PetZone.Listings.Application.Commands.UpdateListing;

public class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
{
    [Obsolete("Obsolete")]
    public UpdateListingCommandValidator()
    {
        RuleFor(c => c.ListingId)
            .NotEmpty()
                .WithErrorCode("listing.id_is_empty")
                .WithMessage("Id оголошення обов'язковий");

        RuleFor(c => c.Title)
            .NotEmpty()
                .WithErrorCode("listing.title_is_empty")
                .WithMessage("Назва оголошення не може бути порожньою")
            .MaximumLength(AdoptionListing.MaxTitleLength)
                .WithErrorCode("listing.title_too_long")
                .WithMessage($"Назва не повинна перевищувати {AdoptionListing.MaxTitleLength} символів");

        RuleFor(c => c.Description)
            .NotEmpty()
                .WithErrorCode("listing.description_is_empty")
                .WithMessage("Опис не може бути порожнім")
            .MaximumLength(AdoptionListing.MaxDescriptionLength)
                .WithErrorCode("listing.description_too_long")
                .WithMessage($"Опис не повинен перевищувати {AdoptionListing.MaxDescriptionLength} символів");

        RuleFor(c => c.Color)
            .NotEmpty()
                .WithErrorCode("listing.color_is_empty")
                .WithMessage("Вкажіть колір тварини")
            .MaximumLength(AdoptionListing.MaxColorLength)
                .WithErrorCode("listing.color_too_long")
                .WithMessage($"Колір не повинен перевищувати {AdoptionListing.MaxColorLength} символів");

        RuleFor(c => c.City)
            .NotEmpty()
                .WithErrorCode("listing.city_is_empty")
                .WithMessage("Вкажіть місто")
            .MaximumLength(AdoptionListing.MaxCityLength)
                .WithErrorCode("listing.city_too_long")
                .WithMessage($"Місто не повинне перевищувати {AdoptionListing.MaxCityLength} символів");

        RuleFor(c => c.SpeciesId)
            .NotEmpty()
                .WithErrorCode("listing.species_is_empty")
                .WithMessage("Вкажіть вид тварини");

        RuleFor(c => c.AgeMonths)
            .GreaterThanOrEqualTo(0)
                .WithErrorCode("listing.age_is_negative")
                .WithMessage("Вік не може бути від'ємним");

        RuleFor(c => c.ContactEmail)
            .EmailAddress()
                .WithErrorCode("listing.contactemail_invalid")
                .WithMessage("Некоректний формат контактного email")
            .When(c => !string.IsNullOrWhiteSpace(c.ContactEmail));
    }
}