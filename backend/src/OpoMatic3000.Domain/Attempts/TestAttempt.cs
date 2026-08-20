using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Domain.Attempts;

public sealed class TestAttempt
{
    private readonly List<TestAttemptTopic> _topics = [];
    private readonly List<TestAttemptQuestion> _questions = [];

    private TestAttempt()
    {
    }

    public TestAttempt(
        Guid submissionId,
        DateTime completedAtUtc,
        int totalQuestions,
        int correctCount,
        int incorrectCount,
        int unansweredCount,
        decimal score,
        short scoringRuleVersion = 1)
    {
        if (submissionId == Guid.Empty)
        {
            throw new ArgumentException("SubmissionId cannot be empty.", nameof(submissionId));
        }

        if (completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The date must be expressed in UTC.", nameof(completedAtUtc));
        }

        if (totalQuestions <= 0 ||
            correctCount < 0 ||
            incorrectCount < 0 ||
            unansweredCount < 0 ||
            correctCount + incorrectCount + unansweredCount != totalQuestions)
        {
            throw new ArgumentException("Attempt counters are inconsistent.");
        }

        if (score is < -2.5m or > 10m)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between -2.5 and 10.");
        }

        if (scoringRuleVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scoringRuleVersion));
        }

        SubmissionId = submissionId;
        CompletedAtUtc = completedAtUtc;
        TotalQuestions = totalQuestions;
        CorrectCount = correctCount;
        IncorrectCount = incorrectCount;
        UnansweredCount = unansweredCount;
        Score = score;
        ScoringRuleVersion = scoringRuleVersion;
    }

    public long Id { get; private set; }

    public Guid SubmissionId { get; private set; }

    public DateTime CompletedAtUtc { get; private set; }

    public int TotalQuestions { get; private set; }

    public int CorrectCount { get; private set; }

    public int IncorrectCount { get; private set; }

    public int UnansweredCount { get; private set; }

    public decimal Score { get; private set; }

    public short ScoringRuleVersion { get; private set; }

    public IReadOnlyCollection<TestAttemptTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyCollection<TestAttemptQuestion> Questions => _questions.AsReadOnly();

    public TestAttemptTopic AddTopic(Topic originalTopic)
    {
        ArgumentNullException.ThrowIfNull(originalTopic);

        if (_topics.Any(topic => ReferenceEquals(topic.OriginalTopic, originalTopic)))
        {
            throw new InvalidOperationException("The topic is already part of this attempt.");
        }

        var attemptTopic = new TestAttemptTopic(this, originalTopic, originalTopic.Name);
        _topics.Add(attemptTopic);
        return attemptTopic;
    }

    public TestAttemptQuestion AddQuestion(
        Question originalQuestion,
        int displayOrder,
        QuestionResult result,
        IEnumerable<TestAttemptOptionDefinition> options)
    {
        ArgumentNullException.ThrowIfNull(originalQuestion);

        var attemptTopic = _topics.SingleOrDefault(
            topic => ReferenceEquals(topic.OriginalTopic, originalQuestion.Topic));

        if (attemptTopic is null)
        {
            throw new InvalidOperationException("The question topic must be selected for the attempt.");
        }

        var question = new TestAttemptQuestion(
            this,
            attemptTopic,
            originalQuestion,
            displayOrder,
            originalQuestion.Statement,
            attemptTopic.TopicNameSnapshot,
            result,
            options);

        _questions.Add(question);
        return question;
    }
}
