using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Application;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        return rule.WithErrorCode(error.Code).WithMessage(error.Description);
    }
}