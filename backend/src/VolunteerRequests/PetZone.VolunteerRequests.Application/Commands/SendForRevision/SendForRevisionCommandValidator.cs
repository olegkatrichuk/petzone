using FluentValidation;

namespace PetZone.VolunteerRequests.Application.Commands.SendForRevision;

public class SendForRevisionCommandValidator : AbstractValidator<SendForRevisionCommand>
{
    [Obsolete("Obsolete")]
    public SendForRevisionCommandValidator()
    {
        RuleFor(c => c.RequestId)
            .NotEmpty()
                .WithErrorCode("volunteer_request.id_empty")
                .WithMessage("Id заявки обов'язковий");

        RuleFor(c => c.Comment)
            .NotEmpty()
                .WithErrorCode("volunteer_request.empty_comment")
                .WithMessage("Коментар обов'язковий")
            .MaximumLength(2000)
                .WithErrorCode("volunteer_request.comment_too_long")
                .WithMessage("Коментар не повинен перевищувати 2000 символів");
    }
}