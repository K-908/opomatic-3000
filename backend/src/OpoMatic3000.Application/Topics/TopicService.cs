using OpoMatic3000.Application.Common.Exceptions;
using OpoMatic3000.Domain.Questions;

namespace OpoMatic3000.Application.Topics;

public sealed class TopicService(ITopicRepository repository, TimeProvider timeProvider)
{
    public Task<IReadOnlyList<TopicDto>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(includeInactive, cancellationToken);

    public async Task<TopicDto> GetAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await repository.GetDetailsAsync(id, cancellationToken) ?? throw TopicNotFound(id);

    public async Task<TopicDto> CreateAsync(
        string? name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = ValidateAndNormalizeName(name);
        await EnsureUniqueNameAsync(normalizedName, null, cancellationToken);

        var topic = new Topic(normalizedName, timeProvider.GetUtcNow().UtcDateTime);
        repository.Add(topic);
        await repository.SaveChangesAsync(cancellationToken);

        return new TopicDto(topic.Id, topic.Name, topic.IsActive, 0);
    }

    public async Task<TopicDto> RenameAsync(
        int id,
        string? name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = ValidateAndNormalizeName(name);
        var topic = await repository.GetAsync(id, cancellationToken) ?? throw TopicNotFound(id);
        await EnsureUniqueNameAsync(normalizedName, id, cancellationToken);

        topic.Rename(normalizedName, timeProvider.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);

        return await repository.GetDetailsAsync(id, cancellationToken)
            ?? throw TopicNotFound(id);
    }

    public async Task SetStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var topic = await repository.GetAsync(id, cancellationToken) ?? throw TopicNotFound(id);

        if (topic.IsActive == isActive)
        {
            return;
        }

        topic.SetActive(isActive, timeProvider.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public static string ValidateAndNormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ValidationError("El nombre del tema es obligatorio.");
        }

        var normalizedName = string.Join(' ', name.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));

        if (normalizedName.Length > 150)
        {
            throw ValidationError("El nombre del tema no puede superar los 150 caracteres.");
        }

        return normalizedName;
    }

    private async Task EnsureUniqueNameAsync(
        string normalizedName,
        int? excludedTopicId,
        CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(normalizedName, excludedTopicId, cancellationToken))
        {
            throw new ResourceConflictException("Ya existe un tema con ese nombre.");
        }
    }

    private static RequestValidationException ValidationError(string message) =>
        new(new Dictionary<string, string[]> { ["name"] = [message] });

    private static ResourceNotFoundException TopicNotFound(int id) =>
        new($"No existe el tema con identificador {id}.");
}
