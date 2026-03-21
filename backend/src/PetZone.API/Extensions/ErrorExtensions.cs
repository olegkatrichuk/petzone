using Microsoft.AspNetCore.Mvc;
using PetZone.API.Envelope;
using PetZone.Domain.Shared;

namespace PetZone.API.Extensions;

public static class ErrorExtensions
{
    public static ActionResult ToResponse(this Error error)
    {
        var statusCode = GetStatusCode(error.Type);
        var envelope = Envelope.Envelope.Error([ErrorInfo.FromError(error)]);
        return new ObjectResult(envelope) { StatusCode = statusCode };
    }

    public static ActionResult ToResponse(this IReadOnlyList<Error> errors)
    {
        var statusCode = GetStatusCode(errors[0].Type);
        var errorInfos = errors.Select(ErrorInfo.FromError);
        var envelope = Envelope.Envelope.Error(errorInfos);
        return new ObjectResult(envelope) { StatusCode = statusCode };
    }

    // NEW
    public static ActionResult ToResponse(this ErrorList errorList)
    {
        return errorList.Errors.ToResponse();
    }

    public static OkObjectResult ToOkResponse(this ControllerBase _, object? result) =>
        new(Envelope.Envelope.Ok(result));

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound   => StatusCodes.Status404NotFound,
        ErrorType.Conflict   => StatusCodes.Status409Conflict,
        ErrorType.Failure    => StatusCodes.Status500InternalServerError,
        _                    => StatusCodes.Status500InternalServerError
    };
}
