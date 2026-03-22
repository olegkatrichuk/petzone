using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Core;

public interface IQueryHandler<in TQuery, TResult>
{
    Task<Result<TResult, ErrorList>> Handle(
        TQuery query,
        CancellationToken cancellationToken = default);
}