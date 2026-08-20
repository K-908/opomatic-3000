using OpoMatic3000.Application.Questions;

namespace OpoMatic3000.Api.Contracts.Questions;

public sealed record SaveQuestionOptionRequest(byte Position, string? Text, bool IsCorrect)
{
    public SaveQuestionOptionDto ToApplication() => new(Position, Text, IsCorrect);
}

public sealed record SaveQuestionRequest(
    int TopicId,
    string? Statement,
    IReadOnlyList<SaveQuestionOptionRequest>? Options);

public sealed record SetQuestionStatusRequest(bool IsActive);

public sealed record QuestionListItemResponse(
    int Id,
    int TopicId,
    string TopicName,
    string Statement,
    bool IsActive,
    DateTime UpdatedAtUtc)
{
    public static QuestionListItemResponse FromApplication(QuestionListItemDto item) =>
        new(item.Id, item.TopicId, item.TopicName, item.Statement, item.IsActive, item.UpdatedAtUtc);
}

public sealed record QuestionOptionResponse(int Id, byte Position, string Text, bool IsCorrect);

public sealed record QuestionResponse(
    int Id,
    int TopicId,
    string Statement,
    bool IsActive,
    IReadOnlyList<QuestionOptionResponse> Options)
{
    public static QuestionResponse FromApplication(QuestionDetailsDto question) =>
        new(
            question.Id,
            question.TopicId,
            question.Statement,
            question.IsActive,
            question.Options.Select(option => new QuestionOptionResponse(
                option.Id,
                option.Position,
                option.Text,
                option.IsCorrect)).ToArray());
}

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
