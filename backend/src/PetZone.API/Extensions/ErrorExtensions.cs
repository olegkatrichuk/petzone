using Microsoft.AspNetCore.Mvc;
using PetZone.Domain.Shared;

namespace PetZone.API.Extensions;

public static class ErrorExtensions
{
    // Ключевое слово this перед Error говорит, что мы расширяем именно этот класс
    public static ActionResult ToResponse(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        // ObjectResult позволяет вернуть сам объект ошибки и задать любой HTTP статус
        return new ObjectResult(error)
        {
            StatusCode = statusCode
        };
    }
}