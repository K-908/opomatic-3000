namespace OpoMatic3000.Domain.Questions;

public sealed class Topic
{
    private readonly List<Question> _questions = [];

    private Topic()
    {
    }

    public Topic(string name, DateTime createdAtUtc)
    {
        Name = NormalizeName(name);
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

    public IReadOnlyCollection<Question> GetAvailableQuestions() =>
        IsActive
            ? _questions.Where(question => question.IsActive).ToArray()
            : [];

    public Question AddQuestion(
        string statement,
        IEnumerable<QuestionOptionDefinition> options,
        DateTime createdAtUtc)
    {
        var question = new Question(this, statement, options, createdAtUtc);
        _questions.Add(question);
        UpdatedAtUtc = createdAtUtc;
        return question;
    }

    public void Rename(string name, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        Name = NormalizeName(name);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SetActive(bool isActive, DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalizedName = string.Join(' ', name.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));

        if (normalizedName.Length > 150)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Topic names cannot exceed 150 characters.");
        }

        return normalizedName;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The date must be expressed in UTC.", parameterName);
        }
    }
}
