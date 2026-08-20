using System.Reflection;
using OpoMatic3000.Application.Common.Exceptions;
using OpoMatic3000.Application.Topics;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.UnitTests.Topics;

public sealed class TopicServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task List_returns_an_empty_collection_when_there_are_no_topics()
    {
        var service = CreateService(new FakeTopicRepository());

        var topics = await service.ListAsync(false);

        Assert.Empty(topics);
    }

    [Fact]
    public async Task List_excludes_inactive_topics_by_default_and_includes_active_question_count()
    {
        var repository = new FakeTopicRepository();
        repository.Seed("Activo", true, 3);
        repository.Seed("Inactivo", false, 2);
        var service = CreateService(repository);

        var topics = await service.ListAsync(false);

        var topic = Assert.Single(topics);
        Assert.Equal("Activo", topic.Name);
        Assert.Equal(3, topic.ActiveQuestionCount);
    }

    [Fact]
    public async Task List_can_include_inactive_topics()
    {
        var repository = new FakeTopicRepository();
        repository.Seed("Activo", true);
        repository.Seed("Inactivo", false);

        var topics = await CreateService(repository).ListAsync(true);

        Assert.Equal(2, topics.Count);
    }

    [Fact]
    public async Task Get_throws_a_specific_error_when_topic_does_not_exist()
    {
        var service = CreateService(new FakeTopicRepository());

        await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.GetAsync(42));
    }

    [Fact]
    public async Task Create_normalizes_spaces_and_returns_the_persisted_topic()
    {
        var repository = new FakeTopicRepository();
        var service = CreateService(repository);

        var topic = await service.CreateAsync("  Derecho   constitucional  ");

        Assert.Equal(1, topic.Id);
        Assert.Equal("Derecho constitucional", topic.Name);
        Assert.True(topic.IsActive);
        Assert.Equal(1, repository.SaveCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_rejects_an_empty_name(string? name)
    {
        var service = CreateService(new FakeTopicRepository());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => service.CreateAsync(name));

        Assert.Contains("name", exception.Errors.Keys);
    }

    [Fact]
    public async Task Create_rejects_duplicate_names_ignoring_case_and_outer_spaces()
    {
        var repository = new FakeTopicRepository();
        repository.Seed("Derecho constitucional", true);
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ResourceConflictException>(
            () => service.CreateAsync("  DERECHO CONSTITUCIONAL "));
    }

    [Fact]
    public async Task Rename_updates_the_name_and_timestamp()
    {
        var repository = new FakeTopicRepository();
        var existing = repository.Seed("Nombre anterior", true);
        var service = CreateService(repository);

        var result = await service.RenameAsync(existing.Id, "Nombre nuevo");

        Assert.Equal("Nombre nuevo", result.Name);
        Assert.Equal(Now.UtcDateTime, existing.UpdatedAtUtc);
    }

    [Fact]
    public async Task Set_status_is_idempotent()
    {
        var repository = new FakeTopicRepository();
        var topic = repository.Seed("Tema", true);
        var service = CreateService(repository);

        await service.SetStatusAsync(topic.Id, true);

        Assert.Equal(0, repository.SaveCount);
        Assert.Equal(topic.CreatedAtUtc, topic.UpdatedAtUtc);
    }

    [Fact]
    public async Task Reactivating_a_topic_preserves_its_active_questions()
    {
        var repository = new FakeTopicRepository();
        var topic = repository.Seed("Tema", false, 4);
        var service = CreateService(repository);

        await service.SetStatusAsync(topic.Id, true);
        var result = await service.GetAsync(topic.Id);

        Assert.True(result.IsActive);
        Assert.Equal(4, result.ActiveQuestionCount);
    }

    private static TopicService CreateService(FakeTopicRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class FakeTopicRepository : ITopicRepository
    {
        private readonly List<(Topic Topic, int ActiveQuestionCount)> _topics = [];
        private int _nextId = 1;

        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<TopicDto>> ListAsync(
            bool includeInactive,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TopicDto>>(_topics
                .Where(item => includeInactive || item.Topic.IsActive)
                .OrderBy(item => item.Topic.Name)
                .Select(ToDto)
                .ToList());

        public Task<TopicDto?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(_topics
                .Where(item => item.Topic.Id == id)
                .Select(ToDto)
                .SingleOrDefault());

        public Task<Topic?> GetAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(_topics.Select(item => item.Topic).SingleOrDefault(topic => topic.Id == id));

        public Task<bool> NameExistsAsync(
            string normalizedName,
            int? excludedTopicId,
            CancellationToken cancellationToken) =>
            Task.FromResult(_topics.Any(item =>
                item.Topic.Id != excludedTopicId &&
                string.Equals(item.Topic.Name, normalizedName, StringComparison.OrdinalIgnoreCase)));

        public void Add(Topic topic)
        {
            SetId(topic, _nextId++);
            _topics.Add((topic, 0));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }

        public Topic Seed(string name, bool isActive, int activeQuestionCount = 0)
        {
            var topic = new Topic(name, Now.AddDays(-1).UtcDateTime);
            SetId(topic, _nextId++);
            if (!isActive)
            {
                topic.SetActive(false, Now.AddHours(-1).UtcDateTime);
            }

            _topics.Add((topic, activeQuestionCount));
            return topic;
        }

        private static TopicDto ToDto((Topic Topic, int ActiveQuestionCount) item) =>
            new(item.Topic.Id, item.Topic.Name, item.Topic.IsActive, item.ActiveQuestionCount);

        private static void SetId(Topic topic, int id) =>
            typeof(Topic).GetProperty(nameof(Topic.Id), BindingFlags.Instance | BindingFlags.Public)!
                .SetValue(topic, id);
    }
}
