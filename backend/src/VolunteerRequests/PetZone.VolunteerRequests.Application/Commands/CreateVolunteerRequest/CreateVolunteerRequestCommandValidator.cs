using FluentValidation;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Commands.CreateVolunteerRequest;

public class CreateVolunteerRequestCommandValidator : AbstractValidator<CreateVolunteerRequestCommand>
{
    [Obsolete("Obsolete")]
    public CreateVolunteerRequestCommandValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty()
                .WithErrorCode("volunteer_request.user_id_empty")
                .WithMessage("UserId обов'язковий");

        RuleFor(c => c.VolunteerInfo)
            .NotNull()
                .WithErrorCode("volunteer_request.info_is_null")
                .WithMessage("Інформація про волонтера обов'язкова");

        When(c => c.VolunteerInfo is not null, () =>
        {
            RuleFor(c => c.VolunteerInfo.Experience)
                .GreaterThanOrEqualTo(0)
                    .WithErrorCode("volunteer_request.experience_negative")
                    .WithMessage("Досвід не може бути від'ємним")
                .LessThanOrEqualTo(80)
                    .WithErrorCode("volunteer_request.experience_too_large")
                    .WithMessage("Досвід не може перевищувати 80 років");

            RuleFor(c => c.VolunteerInfo.Motivation)
                .NotEmpty()
                    .WithErrorCode("volunteer_request.motivation_empty")
                    .WithMessage("Мотивація обов'язкова")
                .MaximumLength(2000)
                    .WithErrorCode("volunteer_request.motivation_too_long")
                    .WithMessage("Мотивація не повинна перевищувати 2000 символів");
        });
    }
}