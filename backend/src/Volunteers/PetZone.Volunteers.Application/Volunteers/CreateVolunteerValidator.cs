using FluentValidation;

namespace PetZone.Volunteers.Application.Volunteers;

public class CreateVolunteerValidator : AbstractValidator<CreateVolunteerCommand>
{
    public CreateVolunteerValidator()
    {
        RuleFor(c => c.Request.FirstName)
            .NotEmpty()
                .WithErrorCode("fullname.firstname_is_empty")
                .WithMessage("Имя обязательно.")
            .MaximumLength(FullName.MAX_FIRST_NAME_LENGTH)
                .WithErrorCode("fullname.firstname_too_long")
                .WithMessage($"Имя не должно превышать {FullName.MAX_FIRST_NAME_LENGTH} символов.");

        RuleFor(c => c.Request.LastName)
            .NotEmpty()
                .WithErrorCode("fullname.lastname_is_empty")
                .WithMessage("Фамилия обязательна.")
            .MaximumLength(FullName.MAX_LAST_NAME_LENGTH)
                .WithErrorCode("fullname.lastname_too_long")
                .WithMessage($"Фамилия не должна превышать {FullName.MAX_LAST_NAME_LENGTH} символов.");

        RuleFor(c => c.Request.Email)
            .NotEmpty()
                .WithErrorCode("email.is_empty")
                .WithMessage("Email не может быть пустым.")
            .MaximumLength(Email.MAX_LENGTH)
                .WithErrorCode("email.too_long")
                .WithMessage($"Email не должен превышать {Email.MAX_LENGTH} символов.")
            .Must(e => e.Contains('@'))
                .WithErrorCode("email.is_invalid")
                .WithMessage("Некорректный формат Email.");

        RuleFor(c => c.Request.Phone)
            .NotEmpty()
                .WithErrorCode("phone.is_empty")
                .WithMessage("Номер телефона не может быть пустым.")
            .MaximumLength(PhoneNumber.MAX_LENGTH)
                .WithErrorCode("phone.too_long")
                .WithMessage($"Номер телефона не должен превышать {PhoneNumber.MAX_LENGTH} символов.");

        RuleFor(c => c.Request.ExperienceYears)
            .GreaterThanOrEqualTo(Experience.MIN_YEARS)
                .WithErrorCode("experience.too_small")
                .WithMessage($"Опыт не может быть меньше {Experience.MIN_YEARS} лет.")
            .LessThanOrEqualTo(Experience.MAX_YEARS)
                .WithErrorCode("experience.too_large")
                .WithMessage($"Опыт не может превышать {Experience.MAX_YEARS} лет.");

        RuleFor(c => c.Request.GeneralDescription)
            .NotEmpty()
                .WithErrorCode("volunteer.description_is_empty")
                .WithMessage("Описание обязательно.")
            .MaximumLength(Volunteer.MaxGeneralDescriptionLength)
                .WithErrorCode("volunteer.description_too_long")
                .WithMessage($"Описание не должно превышать {Volunteer.MaxGeneralDescriptionLength} символов.");

        RuleForEach(c => c.Request.SocialNetworks)
            .ChildRules(sn =>
            {
                sn.RuleFor(x => x.Name)
                    .NotEmpty()
                        .WithErrorCode("social_network.name_is_empty")
                        .WithMessage("Название соцсети обязательно.")
                    .MaximumLength(SocialNetwork.MAX_NAME_LENGTH)
                        .WithErrorCode("social_network.name_too_long")
                        .WithMessage($"Название соцсети не должно превышать {SocialNetwork.MAX_NAME_LENGTH} символов.");

                sn.RuleFor(x => x.Link)
                    .NotEmpty()
                        .WithErrorCode("social_network.link_is_empty")
                        .WithMessage("Ссылка соцсети обязательна.")
                    .MaximumLength(SocialNetwork.MAX_LINK_LENGTH)
                        .WithErrorCode("social_network.link_too_long")
                        .WithMessage($"Ссылка соцсети не должна превышать {SocialNetwork.MAX_LINK_LENGTH} символов.");
            });

        RuleForEach(c => c.Request.Requisites)
            .ChildRules(r =>
            {
                r.RuleFor(x => x.Name)
                    .NotEmpty()
                        .WithErrorCode("requisite.name_is_empty")
                        .WithMessage("Название реквизита обязательно.");

                r.RuleFor(x => x.Description)
                    .NotEmpty()
                        .WithErrorCode("requisite.description_is_empty")
                        .WithMessage("Описание реквизита обязательно.");
            });
    }
}
