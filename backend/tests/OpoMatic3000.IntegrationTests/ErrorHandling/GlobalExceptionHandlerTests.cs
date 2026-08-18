using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OpoMatic3000.Api.ErrorHandling;
using OpoMatic3000.Application.Common.Exceptions;

namespace OpoMatic3000.IntegrationTests.ErrorHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Validation_error_returns_problem_details_with_field_errors()
    {
        var exception = new RequestValidationException(
            new Dictionary<string, string[]>
            {
                ["statement"] = ["El enunciado es obligatorio."]
            });

        var response = await HandleAsync(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.ContentType);
        Assert.Equal(
            "El enunciado es obligatorio.",
            response.Body.GetProperty("errors").GetProperty("statement")[0].GetString());
        Assert.Equal("test-trace-id", response.Body.GetProperty("traceId").GetString());
    }

    [Theory]
    [InlineData("not-found", StatusCodes.Status404NotFound)]
    [InlineData("conflict", StatusCodes.Status409Conflict)]
    public async Task Expected_errors_return_their_documented_status(
        string errorType,
        int expectedStatus)
    {
        Exception exception = errorType switch
        {
            "not-found" => new ResourceNotFoundException("El tema no existe."),
            "conflict" => new ResourceConflictException("El tema ya existe."),
            _ => throw new ArgumentOutOfRangeException(nameof(errorType))
        };

        var response = await HandleAsync(exception);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.ContentType);
        Assert.Equal($"https://opomatic-3000/errors/{errorType}",
            response.Body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task Unexpected_error_returns_safe_problem_details()
    {
        const string sensitiveMessage = "SQL password and internal stack details";

        var response = await HandleAsync(new InvalidOperationException(sensitiveMessage));
        var serializedBody = response.Body.GetRawText();

        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.ContentType);
        Assert.DoesNotContain(sensitiveMessage, serializedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), serializedBody, StringComparison.Ordinal);
        Assert.Equal("test-trace-id", response.Body.GetProperty("traceId").GetString());
    }

    private static async Task<ProblemResponse> HandleAsync(Exception exception)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddProblemDetails();

        await using var serviceProvider = services.BuildServiceProvider();
        var problemDetailsService = serviceProvider.GetRequiredService<IProblemDetailsService>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            TraceIdentifier = "test-trace-id"
        };

        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/api/test";
        httpContext.Response.Body = new MemoryStream();

        var handler = new GlobalExceptionHandler(
            problemDetailsService,
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        Assert.True(handled);

        httpContext.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(httpContext.Response.Body);

        return new ProblemResponse(
            httpContext.Response.StatusCode,
            httpContext.Response.ContentType,
            body.RootElement.Clone());
    }

    private sealed record ProblemResponse(
        int StatusCode,
        string? ContentType,
        JsonElement Body);
}
