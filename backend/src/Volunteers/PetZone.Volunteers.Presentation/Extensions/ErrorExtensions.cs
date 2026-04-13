using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetZone.SharedKernel;
using EnvelopeClass = PetZone.Volunteers.Presentation.Envelope.Envelope;
using PetZone.Volunteers.Presentation.Envelope;

namespace PetZone.Volunteers.Presentation.Extensions;

public static class ErrorExtensions
{
    public static ActionResult ToResponse(this Error error)
    {
        var statusCode = GetStatusCode(error.Type);
        var envelope = EnvelopeClass.Error([ErrorInfo.FromError(error)]);
        return new ObjectResult(envelope) { StatusCode = statusCode };
    }

    public static ActionResult ToResponse(this IReadOnlyList<Error> errors)
    {
        var statusCode = GetStatusCode(errors[0].Type);
        var errorInfos = errors.Select(ErrorInfo.FromError);
        var envelope = EnvelopeClass.Error(errorInfos);
        return new ObjectResult(envelope) { StatusCode = statusCode };
    }

    public static ActionResult ToResponse(this ErrorList errorList)
    {
        return errorList.Errors.ToResponse();
    }

    public static OkObjectResult ToOkResponse(this ControllerBase _, object? result) =>
        new(EnvelopeClass.Ok(result));

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound   => StatusCodes.Status404NotFound,
        ErrorType.Conflict   => StatusCodes.Status409Conflict,
        ErrorType.Forbidden  => StatusCodes.Status403Forbidden,
        ErrorType.Failure    => StatusCodes.Status500InternalServerError,
        _                    => StatusCodes.Status500InternalServerError
    };
}