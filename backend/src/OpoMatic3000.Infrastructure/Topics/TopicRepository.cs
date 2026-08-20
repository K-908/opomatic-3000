using Microsoft.EntityFrameworkCore;
using OpoMatic3000.Application.Topics;
using OpoMatic3000.Domain.Questions;
using OpoMatic3000.Infrastructure.Persistence;

namespace OpoMatic3000.Infrastructure.Topics;

internal sealed class TopicRepository(OpoMatic3000DbContext dbContext) : ITopicRepository
{
    public async Task<IReadOnlyList<TopicDto>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Topics.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(topic => topic.IsActive);
        }

        return await query
            .OrderBy(topic => topic.Name)
            .Select(topic => new TopicDto(
                topic.Id,
                topic.Name,
                topic.IsActive,
                topic.Questions.Count(question => question.IsActive)))
            .ToListAsync(cancellationToken);
    }

    public Task<TopicDto?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Id == id)
            .Select(topic => new TopicDto(
                topic.Id,
                topic.Name,
                topic.IsActive,
                topic.Questions.Count(question => question.IsActive)))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Topic?> GetAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Topics.SingleOrDefaultAsync(topic => topic.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(
        string normalizedName,
        int? excludedTopicId,
        CancellationToken cancellationToken) =>
        dbContext.Topics.AnyAsync(
            topic => topic.Name == normalizedName &&
                (!excludedTopicId.HasValue || topic.Id != excludedTopicId.Value),
            cancellationToken);

    public void Add(Topic topic) => dbContext.Topics.Add(topic);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
