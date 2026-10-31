
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Solicitudes.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            context.Response.ContentType = "application/problem+json";

            var errors = ex.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()
                );

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Se encontraron errores de validación.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception)
        {
            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Ocurrió un error interno en el servidor."
                });
        }
    }
}
