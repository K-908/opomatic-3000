using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpoMatic3000.Application.Topics;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.IntegrationTests.Topics;

public sealed class TopicEndpointsTests
{
    [Fact]
    public async Task Complete_topic_management_flow_matches_the_http_contract()
    {
        await using var factory = new TopicApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/topics",
            new { name = "  Derecho   constitucional " });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TopicResponse>();
        Assert.NotNull(created);
        Assert.Equal("Derecho constitucional", created.Name);
        Assert.Equal($"/api/topics/{created.Id}", createResponse.Headers.Location?.AbsolutePath);

        var list = await client.GetFromJsonAsync<TopicResponse[]>("/api/topics");
        Assert.Single(list!);

        var detail = await client.GetFromJsonAsync<TopicResponse>($"/api/topics/{created.Id}");
        Assert.Equal(created, detail);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/topics/{created.Id}",
            new { name = "Derecho administrativo" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(
            "Derecho administrativo",
            (await updateResponse.Content.ReadFromJsonAsync<TopicResponse>())?.Name);

        var disableResponse = await client.PatchAsJsonAsync(
            $"/api/topics/{created.Id}/status",
            new { isActive = false });
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<TopicResponse[]>("/api/topics"))!);

        var allTopics = await client.GetFromJsonAsync<TopicResponse[]>(
            "/api/topics?includeInactive=true");
        Assert.False(Assert.Single(allTopics!).IsActive);

        var reactivateResponse = await client.PatchAsJsonAsync(
            $"/api/topics/{created.Id}/status",
            new { isActive = true });
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);
    }

    [Fact]
    public async Task Invalid_duplicate_and_missing_topics_return_problem_details()
    {
        await using var factory = new TopicApiFactory();
        using var client = factory.CreateClient();

        var invalid = await client.PostAsJsonAsync("/api/topics", new { name = " " });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);
        var invalidProblem = await invalid.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Contains("name", invalidProblem!.Errors.Keys);

        await client.PostAsJsonAsync("/api/topics", new { name = "Tema único" });
        var duplicate = await client.PostAsJsonAsync("/api/topics", new { name = " TEMA ÚNICO " });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("application/problem+json", duplicate.Content.Headers.ContentType?.MediaType);

        var missing = await client.GetAsync("/api/topics/999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("application/problem+json", missing.Content.Headers.ContentType?.MediaType);

        var missingUpdate = await client.PutAsJsonAsync(
            "/api/topics/999",
            new { name = "Tema inexistente" });
        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);
        Assert.Equal(
            "application/problem+json",
            missingUpdate.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApi_contains_all_topic_operations()
    {
        await using var factory = new TopicApiFactory();
        using var client = factory.CreateClient();

        var document = await client.GetStringAsync("/openapi/v1.json");

        Assert.Contains("/api/topics", document, StringComparison.Ordinal);
        Assert.Contains("/api/topics/{id}", document, StringComparison.Ordinal);
        Assert.Contains("/api/topics/{id}/status", document, StringComparison.Ordinal);
    }

    private sealed record TopicResponse(
        int Id,
        string Name,
        bool IsActive,
        int ActiveQuestionCount);

    private sealed record ProblemResponse(Dictionary<string, string[]> Errors);

    private sealed class TopicApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OpoMatic3000"] =
                        "Server=localhost;Database=NotUsed;Trusted_Connection=True;TrustServerCertificate=True"
                }));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITopicRepository>();
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<ITopicRepository, FakeTopicRepository>();
                services.AddSingleton<TimeProvider>(
                    new FixedTimeProvider(new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero)));
            });
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class FakeTopicRepository : ITopicRepository
    {
        private readonly List<Topic> _topics = [];
        private int _nextId = 1;

        public Task<IReadOnlyList<TopicDto>> ListAsync(
            bool includeInactive,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TopicDto>>(_topics
                .Where(topic => includeInactive || topic.IsActive)
                .OrderBy(topic => topic.Name)
                .Select(ToDto)
                .ToList());

        public Task<TopicDto?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(_topics.Where(topic => topic.Id == id).Select(ToDto).SingleOrDefault());

        public Task<Topic?> GetAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(_topics.SingleOrDefault(topic => topic.Id == id));

        public Task<bool> NameExistsAsync(
            string normalizedName,
            int? excludedTopicId,
            CancellationToken cancellationToken) =>
            Task.FromResult(_topics.Any(topic =>
                topic.Id != excludedTopicId &&
                string.Equals(topic.Name, normalizedName, StringComparison.OrdinalIgnoreCase)));

        public void Add(Topic topic)
        {
            typeof(Topic).GetProperty(nameof(Topic.Id), BindingFlags.Instance | BindingFlags.Public)!
                .SetValue(topic, _nextId++);
            _topics.Add(topic);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static TopicDto ToDto(Topic topic) =>
            new(topic.Id, topic.Name, topic.IsActive, topic.Questions.Count(question => question.IsActive));
    }
}
