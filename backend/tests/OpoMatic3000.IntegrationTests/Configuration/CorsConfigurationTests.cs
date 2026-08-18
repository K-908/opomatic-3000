using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpoMatic3000.Api.Configuration;

namespace OpoMatic3000.IntegrationTests.Configuration;

public sealed class CorsConfigurationTests
{
    [Theory]
    [InlineData("http://localhost:5173", true)]
    [InlineData("http://localhost:4173", false)]
    [InlineData("https://example.com", false)]
    public async Task Policy_only_allows_configured_origins(
        string origin,
        bool expectedAllowed)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApiCors(configuration);

        await using var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = origin;
        var policy = await policyProvider.GetPolicyAsync(
            httpContext,
            CorsConfiguration.FrontendPolicy);

        Assert.NotNull(policy);
        var result = corsService.EvaluatePolicy(httpContext, policy);
        corsService.ApplyResult(result, httpContext.Response);

        Assert.Equal(expectedAllowed, result.IsOriginAllowed);
        Assert.False(policy.AllowAnyOrigin);
        Assert.Equal(
            expectedAllowed,
            httpContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"));

        if (expectedAllowed)
        {
            Assert.Equal(
                origin,
                httpContext.Response.Headers.AccessControlAllowOrigin.ToString());
        }
    }
}
