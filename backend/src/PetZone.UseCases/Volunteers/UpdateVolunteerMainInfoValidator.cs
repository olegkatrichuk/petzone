using FluentValidation;
using PetZone.Domain.Models;
using PetZone.UseCases.Commands;

namespace PetZone.UseCases.Volunteers;

public class UpdateVolunteerMainInfoValidator : AbstractValidator<UpdateVolunteerMainInfoCommand>
{
    [Obsolete("Obsolete")]
    public UpdateVolunteerMainInfoValidator()
    {
        RuleFor(c => c.Request.FirstName)
            .MustBeValueObject(fn => FullName.Create(fn, "placeholder"));

        RuleFor(c => c.Request.LastName)
            .MustBeValueObject(ln => FullName.Create("placeholder", ln));

        RuleFor(c => c.Request.Email)
            .MustBeValueObject(Email.Create);

        RuleFor(c => c.Request.Phone)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(c => c.Request.ExperienceYears)
            .MustBeValueObject(Experience.Create);

        RuleFor(c => c.Request.GeneralDescription)
            .NotEmpty()
            .MaximumLength(Volunteer.MaxGeneralDescriptionLength);
    }
}