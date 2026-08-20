namespace OpoMatic3000.Domain.Questions;

public sealed class Question
{
    private readonly List<QuestionOption> _options = [];

    private Question()
    {
    }

    internal Question(
        Topic topic,
        string statement,
        IEnumerable<QuestionOptionDefinition> options,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(statement);

        if (statement.Trim().Length > 10000)
        {
            throw new ArgumentOutOfRangeException(nameof(statement), "Question statements cannot exceed 10000 characters.");
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The date must be expressed in UTC.", nameof(createdAtUtc));
        }

        var optionList = options?.ToArray() ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(optionList);

        Topic = topic;
        TopicId = topic.Id;
        Statement = statement.Trim();
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        foreach (var option in optionList.OrderBy(option => option.Position))
        {
            _options.Add(new QuestionOption(this, option.Text, option.Position, option.IsCorrect));
        }
    }

    public int Id { get; private set; }

    public int TopicId { get; private set; }

    public Topic Topic { get; private set; } = null!;

    public string Statement { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<QuestionOption> Options => _options.AsReadOnly();

    public void Update(
        Topic topic,
        string statement,
        IEnumerable<QuestionOptionDefinition> options,
        DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(statement);
        if (statement.Trim().Length > 10000)
        {
            throw new ArgumentOutOfRangeException(nameof(statement), "Question statements cannot exceed 10000 characters.");
        }
        if (updatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The date must be expressed in UTC.", nameof(updatedAtUtc));
        }

        var optionList = options?.ToArray() ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(optionList);
        if (_options.Count != 4)
        {
            throw new InvalidOperationException("The persisted question must contain exactly four options.");
        }

        Topic = topic;
        TopicId = topic.Id;
        Statement = statement.Trim();
        foreach (var definition in optionList)
        {
            _options.Single(option => option.Position == definition.Position).Update(
                definition.Text,
                definition.IsCorrect);
        }
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SetActive(bool isActive, DateTime updatedAtUtc)
    {
        if (updatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The date must be expressed in UTC.", nameof(updatedAtUtc));
        }

        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateOptions(IReadOnlyCollection<QuestionOptionDefinition> options)
    {
        if (options.Count != 4)
        {
            throw new ArgumentException("A question must have exactly four options.", nameof(options));
        }

        if (options.Select(option => option.Position).Distinct().Count() != 4 ||
            options.Any(option => option.Position is < 1 or > 4))
        {
            throw new ArgumentException("Option positions must be unique values from 1 to 4.", nameof(options));
        }

        if (options.Count(option => option.IsCorrect) != 1)
        {
            throw new ArgumentException("A question must have exactly one correct option.", nameof(options));
        }
    }
}
