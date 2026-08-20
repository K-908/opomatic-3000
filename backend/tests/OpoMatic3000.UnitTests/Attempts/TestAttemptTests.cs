using OpoMatic3000.Domain.Attempts;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.UnitTests.Attempts;

public sealed class TestAttemptTests
{
    private static readonly DateTime UtcNow = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Attempt_preserves_topic_question_and_option_snapshots()
    {
        var topic = CreateTopicWithQuestion();
        var originalQuestion = Assert.Single(topic.Questions);
        var attempt = new TestAttempt(Guid.NewGuid(), UtcNow, 1, 1, 0, 0, 10m);
        attempt.AddTopic(topic);
        var optionDefinitions = originalQuestion.Options.Select(
            option => new TestAttemptOptionDefinition(option, option.Position, option.IsCorrect));

        var historicalQuestion = attempt.AddQuestion(
            originalQuestion,
            1,
            QuestionResult.Correct,
            optionDefinitions);

        topic.Rename("Changed topic", UtcNow.AddMinutes(1));

        Assert.Equal("Original topic", Assert.Single(attempt.Topics).TopicNameSnapshot);
        Assert.Equal("Original topic", historicalQuestion.TopicNameSnapshot);
        Assert.Equal("Original statement", historicalQuestion.StatementSnapshot);
        Assert.Equal(originalQuestion.Options.Select(option => option.Text),
            historicalQuestion.Options.Select(option => option.TextSnapshot));
    }

    [Fact]
    public void AddQuestion_requires_the_original_topic_to_be_selected()
    {
        var topic = CreateTopicWithQuestion();
        var originalQuestion = Assert.Single(topic.Questions);
        var attempt = new TestAttempt(Guid.NewGuid(), UtcNow, 1, 0, 0, 1, 0m);

        Assert.Throws<InvalidOperationException>(() => attempt.AddQuestion(
            originalQuestion,
            1,
            QuestionResult.Unanswered,
            originalQuestion.Options.Select(
                option => new TestAttemptOptionDefinition(option, option.Position, false))));
    }

    [Fact]
    public void Constructor_rejects_inconsistent_counters()
    {
        Assert.Throws<ArgumentException>(() =>
            new TestAttempt(Guid.NewGuid(), UtcNow, 2, 1, 1, 1, 3.75m));
    }

    private static Topic CreateTopicWithQuestion()
    {
        var topic = new Topic("Original topic", UtcNow);
        topic.AddQuestion(
            "Original statement",
            Enumerable.Range(1, 4).Select(position =>
                new QuestionOptionDefinition($"Option {position}", (byte)position, position == 1)),
            UtcNow);
        return topic;
    }
}
