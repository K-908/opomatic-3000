namespace OpoMatic3000.Application.Questions;

public sealed record QuestionListItemDto(
    int Id,
    int TopicId,
    string TopicName,
    string Statement,
    bool IsActive,
    DateTime UpdatedAtUtc);

public sealed record QuestionOptionDto(
    int Id,
    byte Position,
    string Text,
    bool IsCorrect);

public sealed record QuestionDetailsDto(
    int Id,
    int TopicId,
    string Statement,
    bool IsActive,
    IReadOnlyList<QuestionOptionDto> Options);

public sealed record SaveQuestionOptionDto(
    byte Position,
    string? Text,
    bool IsCorrect);
