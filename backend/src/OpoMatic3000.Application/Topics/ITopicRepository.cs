using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Application.Topics;

public interface ITopicRepository
{
    Task<IReadOnlyList<TopicDto>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<TopicDto?> GetDetailsAsync(int id, CancellationToken cancellationToken);

    Task<Topic?> GetAsync(int id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(
        string normalizedName,
        int? excludedTopicId,
        CancellationToken cancellationToken);

    void Add(Topic topic);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
