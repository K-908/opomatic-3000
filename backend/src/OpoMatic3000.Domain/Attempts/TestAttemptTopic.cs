using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Domain.Attempts;

public sealed class TestAttemptTopic
{
    private TestAttemptTopic()
    {
    }

    internal TestAttemptTopic(TestAttempt testAttempt, Topic originalTopic, string topicNameSnapshot)
    {
        TestAttempt = testAttempt;
        OriginalTopic = originalTopic;
        TopicNameSnapshot = topicNameSnapshot;
    }

    public long TestAttemptId { get; private set; }

    public TestAttempt TestAttempt { get; private set; } = null!;

    public int OriginalTopicId { get; private set; }

    public Topic OriginalTopic { get; private set; } = null!;

    public string TopicNameSnapshot { get; private set; } = string.Empty;
}
