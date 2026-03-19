using System.Net;

namespace PetZone.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Логируем полный стэктрейс — только на сервере
            logger.LogError(ex, "Unhandled exception occurred. RequestPath: {Path}", 
                context.Request.Path);

            // Клиенту возвращаем только текст без стэктрейса
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 500,
                Message = "Internal server error. Please try again later."
            });
        }
    }
}