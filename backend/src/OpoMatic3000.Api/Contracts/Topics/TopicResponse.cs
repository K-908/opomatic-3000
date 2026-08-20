using OpoMatic3000.Application.Topics;

namespace OpoMatic3000.Api.Contracts.Topics;

public sealed record TopicResponse(
    int Id,
    string Name,
    bool IsActive,
    int ActiveQuestionCount)
{
    public static TopicResponse FromApplication(TopicDto topic) =>
        new(topic.Id, topic.Name, topic.IsActive, topic.ActiveQuestionCount);
}
