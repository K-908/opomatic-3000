using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpoMatic3000.Application.Common.Exceptions;

namespace OpoMatic3000.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unexpected error while processing {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                "Request failed with {StatusCode} for {Method} {Path}: {ExceptionType}. TraceId: {TraceId}",
                problemDetails.Status,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.GetType().Name,
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        var problemDetails = exception switch
        {
            RequestValidationException validationException => CreateValidationProblemDetails(
                validationException),
            ResourceNotFoundException notFoundException => new ProblemDetails
            {
                Type = "https://opomatic-3000/errors/not-found",
                Title = "No se ha encontrado el recurso",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundException.Message
            },
            ResourceConflictException conflictException => new ProblemDetails
            {
                Type = "https://opomatic-3000/errors/conflict",
                Title = "La operación entra en conflicto con el estado actual",
                Status = StatusCodes.Status409Conflict,
                Detail = conflictException.Message
            },
            _ => new ProblemDetails
            {
                Type = "https://opomatic-3000/errors/internal",
                Title = "Se ha producido un error inesperado",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "No se ha podido completar la operación."
            }
        };

        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return problemDetails;
    }

    private static ProblemDetails CreateValidationProblemDetails(
        RequestValidationException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://opomatic-3000/errors/validation",
            Title = "Los datos enviados no son válidos",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message
        };

        problemDetails.Extensions["errors"] = exception.Errors;

        return problemDetails;
    }
}
