using System.Diagnostics;

namespace OpoMatic3000.Api.ErrorHandling;

public static class ProblemDetailsConfiguration
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions.TryAdd(
                    "traceId",
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
