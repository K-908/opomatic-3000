using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OpoMatic3000.IntegrationTests.Health;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_returns_ok_with_the_application_status()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(health);
        Assert.Equal("ok", health.Status);
        Assert.Equal("OpoMatic-3000", health.Application);
    }

    private sealed record HealthResponse(string Status, string Application);
}
