using System.Reflection;
using OpoMatic3000.Application.Common.Exceptions;
using OpoMatic3000.Application.Questions;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.UnitTests.Questions;

public sealed class QuestionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task List_combines_filters_and_returns_empty_pages_with_metadata()
    {
        var repository = new FakeQuestionRepository();
        var topic = repository.SeedTopic("Tema");
        repository.SeedQuestion(topic, "Primera pregunta", true);
        repository.SeedQuestion(topic, "Segunda pregunta", false);
        var service = CreateService(repository);

        var filtered = await service.ListAsync(topic.Id, true, "segunda", 1, 20);
        var emptyPage = await service.ListAsync(topic.Id, true, null, 2, 20);

        Assert.Equal("Segunda pregunta", Assert.Single(filtered.Items).Statement);
        Assert.Empty(emptyPage.Items);
        Assert.Equal(2, emptyPage.TotalItems);
        Assert.Equal(1, emptyPage.TotalPages);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task List_rejects_invalid_page_limits(int page, int pageSize)
    {
        await Assert.ThrowsAsync<RequestValidationException>(
            () => CreateService(new FakeQuestionRepository()).ListAsync(null, false, null, page, pageSize));
    }

    [Fact]
    public async Task Create_saves_exactly_four_options_and_one_correct_answer()
    {
        var repository = new FakeQuestionRepository();
        var topic = repository.SeedTopic("Tema");

        var created = await CreateService(repository).CreateAsync(topic.Id, " Pregunta ", ValidOptions());

        Assert.Equal("Pregunta", created.Statement);
        Assert.Equal(4, created.Options.Count);
        Assert.Single(created.Options, option => option.IsCorrect);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Create_rejects_invalid_options_without_persisting_partial_data()
    {
        var repository = new FakeQuestionRepository();
        var topic = repository.SeedTopic("Tema");
        var invalid = ValidOptions().Select(option => option with { IsCorrect = false }).ToArray();

        await Assert.ThrowsAsync<RequestValidationException>(
            () => CreateService(repository).CreateAsync(topic.Id, "Pregunta", invalid));

        Assert.Empty(repository.Questions);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Create_rejects_an_inactive_topic()
    {
        var repository = new FakeQuestionRepository();
        var topic = repository.SeedTopic("Tema", false);

        await Assert.ThrowsAsync<ResourceConflictException>(
            () => CreateService(repository).CreateAsync(topic.Id, "Pregunta", ValidOptions()));
    }

    [Fact]
    public async Task Update_preserves_option_ids_and_changes_topic_and_correct_answer()
    {
        var repository = new FakeQuestionRepository();
        var firstTopic = repository.SeedTopic("Tema uno");
        var secondTopic = repository.SeedTopic("Tema dos");
        var question = repository.SeedQuestion(firstTopic, "Original", true);
        var optionIds = question.Options.OrderBy(option => option.Position).Select(option => option.Id).ToArray();
        var updatedOptions = ValidOptions(4);

        var updated = await CreateService(repository).UpdateAsync(
            question.Id, secondTopic.Id, "Actualizada", updatedOptions);

        Assert.Equal(secondTopic.Id, updated.TopicId);
        Assert.Equal(optionIds, updated.Options.Select(option => option.Id));
        Assert.True(updated.Options.Single(option => option.Position == 4).IsCorrect);
        Assert.Equal(4, question.Options.Count);
    }

    [Fact]
    public async Task Reactivation_is_rejected_when_the_topic_is_inactive()
    {
        var repository = new FakeQuestionRepository();
        var topic = repository.SeedTopic("Tema");
        var question = repository.SeedQuestion(topic, "Pregunta", false);
        topic.SetActive(false, Now.UtcDateTime);

        await Assert.ThrowsAsync<ResourceConflictException>(
            () => CreateService(repository).SetStatusAsync(question.Id, true));
    }

    private static QuestionService CreateService(FakeQuestionRepository repository) =>
        new(repository, new FixedTimeProvider(Now));

    private static SaveQuestionOptionDto[] ValidOptions(byte correctPosition = 2) =>
        Enumerable.Range(1, 4)
            .Select(position => new SaveQuestionOptionDto(
                (byte)position, $"Opción {position}", position == correctPosition))
            .ToArray();

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }

    private sealed class FakeQuestionRepository : IQuestionRepository
    {
        private readonly List<Topic> _topics = [];
        private int _nextTopicId = 1;
        private int _nextQuestionId = 1;
        private int _nextOptionId = 1;

        public IReadOnlyList<Question> Questions => _topics.SelectMany(topic => topic.Questions).ToArray();
        public int SaveCount { get; private set; }

        public Task<PagedResult<QuestionListItemDto>> ListAsync(
            int? topicId, bool includeInactive, string? search, int page, int pageSize,
            CancellationToken cancellationToken)
        {
            var query = Questions.AsEnumerable();
            if (topicId.HasValue) query = query.Where(question => question.TopicId == topicId);
            if (!includeInactive) query = query.Where(question => question.IsActive);
            if (search is not null) query = query.Where(question => question.Statement.Contains(search, StringComparison.OrdinalIgnoreCase));
            var all = query.OrderByDescending(question => question.UpdatedAtUtc).ThenBy(question => question.Id).ToArray();
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(ToListItem).ToArray();
            return Task.FromResult(new PagedResult<QuestionListItemDto>(
                items, page, pageSize, all.Length, (int)Math.Ceiling(all.Length / (double)pageSize)));
        }

        public Task<QuestionDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(Questions.Where(question => question.Id == id).Select(ToDetails).SingleOrDefault());

        public Task<Question?> GetAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(Questions.SingleOrDefault(question => question.Id == id));

        public Task<Topic?> GetTopicAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(_topics.SingleOrDefault(topic => topic.Id == id));

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            foreach (var question in Questions.Where(question => question.Id == 0))
            {
                SetId(question, _nextQuestionId++);
                foreach (var option in question.Options) SetId(option, _nextOptionId++);
            }
            SaveCount++;
            return Task.CompletedTask;
        }

        public Topic SeedTopic(string name, bool isActive = true)
        {
            var topic = new Topic(name, Now.AddDays(-1).UtcDateTime);
            SetId(topic, _nextTopicId++);
            if (!isActive) topic.SetActive(false, Now.AddHours(-1).UtcDateTime);
            _topics.Add(topic);
            return topic;
        }

        public Question SeedQuestion(Topic topic, string statement, bool isActive)
        {
            var question = topic.AddQuestion(
                statement,
                ValidOptions().Select(option => new QuestionOptionDefinition(option.Text!, option.Position, option.IsCorrect)),
                Now.AddHours(-2).UtcDateTime);
            SetId(question, _nextQuestionId++);
            foreach (var option in question.Options) SetId(option, _nextOptionId++);
            if (!isActive) question.SetActive(false, Now.AddHours(-1).UtcDateTime);
            return question;
        }

        private static QuestionListItemDto ToListItem(Question question) =>
            new(question.Id, question.TopicId, question.Topic.Name, question.Statement, question.IsActive, question.UpdatedAtUtc);

        private static QuestionDetailsDto ToDetails(Question question) =>
            new(question.Id, question.TopicId, question.Statement, question.IsActive,
                question.Options.OrderBy(option => option.Position)
                    .Select(option => new QuestionOptionDto(option.Id, option.Position, option.Text, option.IsCorrect)).ToArray());

        private static void SetId(object entity, int id) => entity.GetType()
            .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)!.SetValue(entity, id);
    }
}
