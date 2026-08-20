using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Domain.Attempts;

public sealed class TestAttemptQuestion
{
    private readonly List<TestAttemptOption> _options = [];

    private TestAttemptQuestion()
    {
    }

    internal TestAttemptQuestion(
        TestAttempt testAttempt,
        TestAttemptTopic attemptTopic,
        Question originalQuestion,
        int displayOrder,
        string statementSnapshot,
        string topicNameSnapshot,
        QuestionResult result,
        IEnumerable<TestAttemptOptionDefinition> options)
    {
        if (displayOrder <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder));
        }

        if (!Enum.IsDefined(result))
        {
            throw new ArgumentOutOfRangeException(nameof(result));
        }

        var optionList = options?.ToArray() ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(originalQuestion, optionList);

        TestAttempt = testAttempt;
        AttemptTopic = attemptTopic;
        OriginalQuestion = originalQuestion;
        OriginalTopic = originalQuestion.Topic;
        DisplayOrder = displayOrder;
        StatementSnapshot = statementSnapshot;
        TopicNameSnapshot = topicNameSnapshot;
        Result = result;

        foreach (var option in optionList.OrderBy(option => option.DisplayOrder))
        {
            _options.Add(new TestAttemptOption(this, option));
        }
    }

    public long Id { get; private set; }

    public long TestAttemptId { get; private set; }

    public TestAttempt TestAttempt { get; private set; } = null!;

    public int OriginalQuestionId { get; private set; }

    public Question OriginalQuestion { get; private set; } = null!;

    public int OriginalTopicId { get; private set; }

    public Topic OriginalTopic { get; private set; } = null!;

    public TestAttemptTopic AttemptTopic { get; private set; } = null!;

    public int DisplayOrder { get; private set; }

    public string StatementSnapshot { get; private set; } = string.Empty;

    public string TopicNameSnapshot { get; private set; } = string.Empty;

    public QuestionResult Result { get; private set; }

    public IReadOnlyCollection<TestAttemptOption> Options => _options.AsReadOnly();

    private static void ValidateOptions(
        Question originalQuestion,
        IReadOnlyCollection<TestAttemptOptionDefinition> options)
    {
        if (options.Count != 4 ||
            options.Select(option => option.DisplayOrder).Distinct().Count() != 4 ||
            options.Any(option => option.DisplayOrder is < 1 or > 4))
        {
            throw new ArgumentException("Historical options must use each display order from 1 to 4.", nameof(options));
        }

        if (options.Select(option => option.OriginalOption).Distinct().Count() != 4 ||
            options.Any(option => !originalQuestion.Options.Contains(option.OriginalOption)))
        {
            throw new ArgumentException("Historical options must belong to the original question.", nameof(options));
        }

        if (options.Count(option => option.IsSelected) > 1)
        {
            throw new ArgumentException("At most one historical option may be selected.", nameof(options));
        }
    }
}
