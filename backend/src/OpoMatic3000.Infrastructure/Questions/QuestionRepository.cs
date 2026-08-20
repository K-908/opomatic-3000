using Microsoft.EntityFrameworkCore;
using OpoMatic3000.Application.Questions;
using OpoMatic3000.Domain.Questions;
using OpoMatic3000.Infrastructure.Persistence;

namespace OpoMatic3000.Infrastructure.Questions;

internal sealed class QuestionRepository(OpoMatic3000DbContext dbContext) : IQuestionRepository
{
    public async Task<PagedResult<QuestionListItemDto>> ListAsync(
        int? topicId,
        bool includeInactive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Questions.AsNoTracking();
        if (topicId.HasValue) query = query.Where(question => question.TopicId == topicId.Value);
        if (!includeInactive) query = query.Where(question => question.IsActive);
        if (search is not null) query = query.Where(question => question.Statement.Contains(search));

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(question => question.UpdatedAtUtc)
            .ThenBy(question => question.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(question => new QuestionListItemDto(
                question.Id,
                question.TopicId,
                question.Topic.Name,
                question.Statement,
                question.IsActive,
                question.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<QuestionListItemDto>(
            items,
            page,
            pageSize,
            totalItems,
            (int)Math.Ceiling(totalItems / (double)pageSize));
    }

    public Task<QuestionDetailsDto?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Questions.AsNoTracking()
            .Where(question => question.Id == id)
            .Select(question => new QuestionDetailsDto(
                question.Id,
                question.TopicId,
                question.Statement,
                question.IsActive,
                question.Options.OrderBy(option => option.Position)
                    .Select(option => new QuestionOptionDto(option.Id, option.Position, option.Text, option.IsCorrect))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Question?> GetAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Questions
            .Include(question => question.Topic)
            .Include(question => question.Options)
            .SingleOrDefaultAsync(question => question.Id == id, cancellationToken);

    public Task<Topic?> GetTopicAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Topics.SingleOrDefaultAsync(topic => topic.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
