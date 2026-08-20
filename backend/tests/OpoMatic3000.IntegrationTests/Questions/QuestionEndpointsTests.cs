using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpoMatic3000.Application.Questions;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.IntegrationTests.Questions;

public sealed class QuestionEndpointsTests
{
    [Fact]
    public async Task Complete_question_management_flow_matches_the_http_contract()
    {
        await using var factory = new QuestionApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/questions", SaveRequest("Primera pregunta", 2));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestionResponse>();
        Assert.NotNull(created);
        Assert.Equal(4, created.Options.Count);
        Assert.Single(created.Options, option => option.IsCorrect);
        Assert.Equal($"/api/questions/{created.Id}", createResponse.Headers.Location?.AbsolutePath);

        var list = await client.GetFromJsonAsync<PagedResponse>(
            "/api/questions?topicId=1&search=Primera&page=1&pageSize=10");
        Assert.Single(list!.Items);
        Assert.Equal(1, list.TotalItems);
        Assert.Equal(1, list.TotalPages);

        var originalOptionIds = created.Options.OrderBy(option => option.Position).Select(option => option.Id).ToArray();
        var updateResponse = await client.PutAsJsonAsync(
            $"/api/questions/{created.Id}", SaveRequest("Pregunta actualizada", 4));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuestionResponse>();
        Assert.Equal(originalOptionIds, updated!.Options.Select(option => option.Id));
        Assert.True(updated.Options.Single(option => option.Position == 4).IsCorrect);

        var disable = await client.PatchAsJsonAsync(
            $"/api/questions/{created.Id}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.NoContent, disable.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<PagedResponse>("/api/questions"))!.Items);
        Assert.Single((await client.GetFromJsonAsync<PagedResponse>(
            "/api/questions?includeInactive=true"))!.Items);
    }

    [Fact]
    public async Task Invalid_and_missing_questions_return_problem_details()
    {
        await using var factory = new QuestionApiFactory();
        using var client = factory.CreateClient();

        var invalid = await client.PostAsJsonAsync("/api/questions", new
        {
            topicId = 1,
            statement = " ",
            options = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);
        var problem = await invalid.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Contains("statement", problem!.Errors.Keys);
        Assert.Contains("options", problem.Errors.Keys);

        var badPage = await client.GetAsync("/api/questions?page=0&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, badPage.StatusCode);

        var missing = await client.GetAsync("/api/questions/999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("application/problem+json", missing.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApi_contains_all_question_operations()
    {
        await using var factory = new QuestionApiFactory();
        using var client = factory.CreateClient();

        var document = await client.GetStringAsync("/openapi/v1.json");

        Assert.Contains("/api/questions", document, StringComparison.Ordinal);
        Assert.Contains("/api/questions/{id}", document, StringComparison.Ordinal);
        Assert.Contains("/api/questions/{id}/status", document, StringComparison.Ordinal);
    }

    private static object SaveRequest(string statement, int correctPosition) => new
    {
        topicId = 1,
        statement,
        options = Enumerable.Range(1, 4).Select(position => new
        {
            position,
            text = $"Opción {position}",
            isCorrect = position == correctPosition
        })
    };

    private sealed record PagedResponse(
        IReadOnlyList<QuestionListItemResponse> Items,
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages);
    private sealed record QuestionListItemResponse(int Id, string Statement, bool IsActive);
    private sealed record QuestionResponse(
        int Id, int TopicId, string Statement, bool IsActive, IReadOnlyList<OptionResponse> Options);
    private sealed record OptionResponse(int Id, byte Position, string Text, bool IsCorrect);
    private sealed record ProblemResponse(Dictionary<string, string[]> Errors);

    private sealed class QuestionApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OpoMatic3000"] =
                        "Server=localhost;Database=NotUsed;Trusted_Connection=True;TrustServerCertificate=True"
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IQuestionRepository>();
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<IQuestionRepository, FakeQuestionRepository>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(
                    new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero)));
            });
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class FakeQuestionRepository : IQuestionRepository
    {
        private static readonly DateTime Now = new(2026, 8, 20, 10, 0, 0, DateTimeKind.Utc);
        private readonly Topic _topic;
        private int _nextQuestionId = 1;
        private int _nextOptionId = 1;

        public FakeQuestionRepository()
        {
            _topic = new Topic("Tema", Now.AddDays(-1));
            SetId(_topic, 1);
        }

        private IEnumerable<Question> Questions => _topic.Questions;

        public Task<PagedResult<QuestionListItemDto>> ListAsync(
            int? topicId, bool includeInactive, string? search, int page, int pageSize,
            CancellationToken cancellationToken)
        {
            var query = Questions.Where(question => !topicId.HasValue || question.TopicId == topicId);
            if (!includeInactive) query = query.Where(question => question.IsActive);
            if (search is not null) query = query.Where(question => question.Statement.Contains(search, StringComparison.OrdinalIgnoreCase));
            var all = query.OrderByDescending(question => question.UpdatedAtUtc).ThenBy(question => question.Id).ToArray();
            var items = all.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(question => new QuestionListItemDto(
                    question.Id, question.TopicId, question.Topic.Name, question.Statement,
                    question.IsActive, question.UpdatedAtUtc)).ToArray();
            return Task.FromResult(new PagedResult<QuestionListItemDto>(
                items, page, pageSize, all.Length, (int)Math.Ceiling(all.Length / (double)pageSize)));
        }

        public Task<QuestionDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(Questions.Where(question => question.Id == id).Select(ToDetails).SingleOrDefault());

        public Task<Question?> GetAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(Questions.SingleOrDefault(question => question.Id == id));

        public Task<Topic?> GetTopicAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(id == _topic.Id ? _topic : null);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            foreach (var question in Questions.Where(question => question.Id == 0))
            {
                SetId(question, _nextQuestionId++);
                foreach (var option in question.Options) SetId(option, _nextOptionId++);
            }
            return Task.CompletedTask;
        }

        private static QuestionDetailsDto ToDetails(Question question) =>
            new(question.Id, question.TopicId, question.Statement, question.IsActive,
                question.Options.OrderBy(option => option.Position)
                    .Select(option => new QuestionOptionDto(option.Id, option.Position, option.Text, option.IsCorrect)).ToArray());

        private static void SetId(object entity, int id) => entity.GetType()
            .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)!.SetValue(entity, id);
    }
}
