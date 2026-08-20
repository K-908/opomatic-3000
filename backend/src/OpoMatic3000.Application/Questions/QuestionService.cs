using OpoMatic3000.Application.Common.Exceptions;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Application.Questions;

public sealed class QuestionService(IQuestionRepository repository, TimeProvider timeProvider)
{
    public Task<PagedResult<QuestionListItemDto>> ListAsync(
        int? topicId,
        bool includeInactive,
        string? search,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();
        if (topicId is <= 0) errors["topicId"] = ["El tema debe ser válido."];
        if (page < 1) errors["page"] = ["La página debe ser mayor o igual que 1."];
        if (pageSize is < 1 or > 100) errors["pageSize"] = ["El tamaño de página debe estar entre 1 y 100."];
        if (errors.Count > 0) throw new RequestValidationException(errors);

        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        return repository.ListAsync(
            topicId,
            includeInactive,
            normalizedSearch,
            page,
            pageSize,
            cancellationToken);
    }

    public async Task<QuestionDetailsDto> GetAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await repository.GetDetailsAsync(id, cancellationToken) ?? throw QuestionNotFound(id);

    public async Task<QuestionDetailsDto> CreateAsync(
        int topicId,
        string? statement,
        IReadOnlyCollection<SaveQuestionOptionDto>? options,
        CancellationToken cancellationToken = default)
    {
        var definitions = Validate(statement, options);
        var topic = await GetActiveTopicAsync(topicId, cancellationToken);
        var question = topic.AddQuestion(
            statement!.Trim(),
            definitions,
            timeProvider.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetDetailsAsync(question.Id, cancellationToken)
            ?? ToDetails(question);
    }

    public async Task<QuestionDetailsDto> UpdateAsync(
        int id,
        int topicId,
        string? statement,
        IReadOnlyCollection<SaveQuestionOptionDto>? options,
        CancellationToken cancellationToken = default)
    {
        var definitions = Validate(statement, options);
        var question = await repository.GetAsync(id, cancellationToken) ?? throw QuestionNotFound(id);
        var topic = await GetActiveTopicAsync(topicId, cancellationToken);
        question.Update(
            topic,
            statement!.Trim(),
            definitions,
            timeProvider.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetDetailsAsync(id, cancellationToken) ?? ToDetails(question);
    }

    public async Task SetStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var question = await repository.GetAsync(id, cancellationToken) ?? throw QuestionNotFound(id);
        if (question.IsActive == isActive) return;
        if (isActive && !question.Topic.IsActive)
        {
            throw new ResourceConflictException("No se puede activar una pregunta cuyo tema está inactivo.");
        }

        question.SetActive(isActive, timeProvider.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Topic> GetActiveTopicAsync(int topicId, CancellationToken cancellationToken)
    {
        var topic = await repository.GetTopicAsync(topicId, cancellationToken)
            ?? throw new ResourceNotFoundException($"No existe el tema con identificador {topicId}.");
        if (!topic.IsActive)
        {
            throw new ResourceConflictException("La pregunta debe pertenecer a un tema activo.");
        }
        return topic;
    }

    private static IReadOnlyList<QuestionOptionDefinition> Validate(
        string? statement,
        IReadOnlyCollection<SaveQuestionOptionDto>? options)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(statement))
            errors["statement"] = ["El enunciado es obligatorio."];
        else if (statement.Trim().Length > 10000)
            errors["statement"] = ["El enunciado no puede superar los 10.000 caracteres."];

        if (options is null || options.Count != 4)
        {
            errors["options"] = ["La pregunta debe contener exactamente cuatro opciones."];
        }
        else
        {
            if (options.Select(option => option.Position).Order().SequenceEqual(new byte[] { 1, 2, 3, 4 }) is false)
                errors["options"] = ["Las posiciones de las opciones deben ser 1, 2, 3 y 4 sin repetirse."];
            if (options.Count(option => option.IsCorrect) != 1)
                errors["correctOption"] = ["Debe marcarse exactamente una respuesta correcta."];
            var invalidText = options.FirstOrDefault(option => string.IsNullOrWhiteSpace(option.Text) || option.Text.Trim().Length > 1000);
            if (invalidText is not null)
                errors[$"options[{invalidText.Position}].text"] = ["El texto de cada opción es obligatorio y no puede superar los 1.000 caracteres."];
        }

        if (errors.Count > 0) throw new RequestValidationException(errors);
        return options!
            .OrderBy(option => option.Position)
            .Select(option => new QuestionOptionDefinition(option.Text!.Trim(), option.Position, option.IsCorrect))
            .ToArray();
    }

    private static QuestionDetailsDto ToDetails(Question question) =>
        new(
            question.Id,
            question.TopicId,
            question.Statement,
            question.IsActive,
            question.Options.OrderBy(option => option.Position)
                .Select(option => new QuestionOptionDto(option.Id, option.Position, option.Text, option.IsCorrect))
                .ToArray());

    private static ResourceNotFoundException QuestionNotFound(int id) =>
        new($"No existe la pregunta con identificador {id}.");
}
