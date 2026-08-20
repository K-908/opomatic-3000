using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Application.Questions;

public interface IQuestionRepository
{
    Task<PagedResult<QuestionListItemDto>> ListAsync(
        int? topicId,
        bool includeInactive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<QuestionDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken);

    Task<Question?> GetAsync(int id, CancellationToken cancellationToken);

    Task<Topic?> GetTopicAsync(int id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
