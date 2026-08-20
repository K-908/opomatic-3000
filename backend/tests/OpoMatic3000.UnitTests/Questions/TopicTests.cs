using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.UnitTests.Questions;

public sealed class TopicTests
{
    private static readonly DateTime UtcNow = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_normalizes_the_name_and_sets_audit_fields()
    {
        var topic = new Topic("  Constitutional law  ", UtcNow);

        Assert.Equal("Constitutional law", topic.Name);
        Assert.True(topic.IsActive);
        Assert.Equal(UtcNow, topic.CreatedAtUtc);
        Assert.Equal(UtcNow, topic.UpdatedAtUtc);
    }

    [Fact]
    public void AddQuestion_creates_exactly_four_ordered_options_with_one_correct()
    {
        var topic = new Topic("Topic 1", UtcNow);
        var options = new[]
        {
            new QuestionOptionDefinition("Fourth", 4, false),
            new QuestionOptionDefinition("Second", 2, true),
            new QuestionOptionDefinition("First", 1, false),
            new QuestionOptionDefinition("Third", 3, false)
        };

        var question = topic.AddQuestion("Question?", options, UtcNow);

        Assert.Single(topic.Questions);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, question.Options.Select(option => option.Position));
        Assert.Single(question.Options, option => option.IsCorrect);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void AddQuestion_rejects_a_number_of_options_other_than_four(int optionCount)
    {
        var topic = new Topic("Topic 1", UtcNow);
        var options = Enumerable.Range(1, optionCount)
            .Select(position => new QuestionOptionDefinition($"Option {position}", (byte)position, position == 1));

        Assert.Throws<ArgumentException>(() => topic.AddQuestion("Question?", options, UtcNow));
    }

    [Fact]
    public void AddQuestion_rejects_more_than_one_correct_option()
    {
        var topic = new Topic("Topic 1", UtcNow);
        var options = Enumerable.Range(1, 4)
            .Select(position => new QuestionOptionDefinition($"Option {position}", (byte)position, position <= 2));

        Assert.Throws<ArgumentException>(() => topic.AddQuestion("Question?", options, UtcNow));
    }
}
