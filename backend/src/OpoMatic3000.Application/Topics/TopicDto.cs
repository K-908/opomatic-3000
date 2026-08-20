namespace OpoMatic3000.Application.Topics;

public sealed record TopicDto(
    int Id,
    string Name,
    bool IsActive,
    int ActiveQuestionCount);
