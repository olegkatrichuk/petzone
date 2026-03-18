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

    /// <summary>
    /// Привязывает доменную ошибку к правилу FluentValidation для не-VO свойств.
    /// Устанавливает ErrorCode и Message в соответствии с нашим типом Error.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        return rule.WithErrorCode(error.Code).WithMessage(error.Description);
    }
}