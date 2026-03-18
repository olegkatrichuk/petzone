using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using PetZone.Domain.Shared;

namespace PetZone.UseCases;

public static class CustomValidators
{
    [Obsolete("Obsolete")]
    public static IRuleBuilderOptionsConditions<T, TProperty> MustBeValueObject<T, TProperty, TValueObject>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<TProperty, Result<TValueObject, Error>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            var result = factoryMethod(value);

            if (result.IsFailure)
            {
                context.AddFailure(new ValidationFailure(context.PropertyName, result.Error.Description)
                {
                    ErrorCode = result.Error.Code
                });
            }
        });
    }
}