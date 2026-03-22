using FluentValidation;

namespace PetZone.Volunteers.Application.Volunteers;

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

        // Не-VO свойство — используем WithError, чтобы вернуть доменную ошибку
        RuleFor(c => c.Request.GeneralDescription)
            .NotEmpty()
            .WithError(Error.Validation("volunteer.description_is_empty", "Описание обязательно."))
            .MaximumLength(Volunteer.MaxGeneralDescriptionLength)
            .WithError(Error.Validation(
                "volunteer.description_too_long",
                $"Описание не должно превышать {Volunteer.MaxGeneralDescriptionLength} символов."));

        // Коллекции — не-VO свойства, используем WithError
        RuleForEach(c => c.Request.SocialNetworks)
            .ChildRules(sn =>
            {
                sn.RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithError(Error.Validation("social_network.name_is_empty", "Название соцсети обязательно."))
                    .MaximumLength(SocialNetwork.MAX_NAME_LENGTH)
                    .WithError(Error.Validation(
                        "social_network.name_too_long",
                        $"Название соцсети не должно превышать {SocialNetwork.MAX_NAME_LENGTH} символов."));

                sn.RuleFor(x => x.Link)
                    .NotEmpty()
                    .WithError(Error.Validation("social_network.link_is_empty", "Ссылка соцсети обязательна."))
                    .MaximumLength(SocialNetwork.MAX_LINK_LENGTH)
                    .WithError(Error.Validation(
                        "social_network.link_too_long",
                        $"Ссылка соцсети не должна превышать {SocialNetwork.MAX_LINK_LENGTH} символов."));
            });

        RuleForEach(c => c.Request.Requisites)
            .ChildRules(r =>
            {
                r.RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithError(Error.Validation("requisite.name_is_empty", "Название реквизита обязательно."));

                r.RuleFor(x => x.Description)
                    .NotEmpty()
                    .WithError(Error.Validation("requisite.description_is_empty", "Описание реквизита обязательно."));
            });
    }
}
