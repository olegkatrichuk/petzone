using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Core;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult, ErrorList>> Handle(
        TCommand command,
        CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
{
    Task<Result<bool, ErrorList>> Handle(
        TCommand command,
        CancellationToken cancellationToken = default);
}