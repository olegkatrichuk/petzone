
using FluentValidation;
using PetZone.Domain.Models;
using PetZone.UseCases;
using PetZone.UseCases.Commands;

namespace PetZone.UseCases.Volunteers;

public class CreateVolunteerValidator : AbstractValidator<CreateVolunteerCommand>
{
    [Obsolete("Obsolete")]
    public CreateVolunteerValidator()
    {
        // FullName — делегируем валидацию фабричному методу Domain
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
            .NotEmpty().WithMessage("Описание обязательно.")
            .MaximumLength(Volunteer.MaxGeneralDescriptionLength)
            .WithMessage($"Описание не должно превышать {Volunteer.MaxGeneralDescriptionLength} символов.");

        // Коллекции
        RuleForEach(c => c.Request.SocialNetworks)
            .ChildRules(sn =>
            {
                sn.RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(SocialNetwork.MAX_NAME_LENGTH);
            
                sn.RuleFor(x => x.Link)
                    .NotEmpty()
                    .MaximumLength(SocialNetwork.MAX_LINK_LENGTH);
            });

        RuleForEach(c => c.Request.Requisites)
            .ChildRules(r =>
            {
                r.RuleFor(x => x.Name).NotEmpty();
                r.RuleFor(x => x.Description).NotEmpty();
            });
    }
}