using Microsoft.AspNetCore.Mvc;
using OpoMatic3000.Api.Contracts.Topics;
using OpoMatic3000.Application.Topics;

namespace OpoMatic3000.Api.Controllers;

[ApiController]
[Route("api/topics")]
[Produces("application/json")]
public sealed class TopicsController(TopicService topicService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TopicResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var topics = await topicService.ListAsync(includeInactive, cancellationToken);
        return Ok(topics.Select(TopicResponse.FromApplication));
    }

    [HttpGet("{id:int}", Name = nameof(GetTopic))]
    [ProducesResponseType<TopicResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TopicResponse>> GetTopic(
        int id,
        CancellationToken cancellationToken)
    {
        var topic = await topicService.GetAsync(id, cancellationToken);
        return Ok(TopicResponse.FromApplication(topic));
    }

    [HttpPost]
    [ProducesResponseType<TopicResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TopicResponse>> Create(
        SaveTopicRequest request,
        CancellationToken cancellationToken)
    {
        var topic = TopicResponse.FromApplication(
            await topicService.CreateAsync(request.Name, cancellationToken));

        return CreatedAtRoute(nameof(GetTopic), new { id = topic.Id }, topic);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<TopicResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TopicResponse>> Rename(
        int id,
        SaveTopicRequest request,
        CancellationToken cancellationToken)
    {
        var topic = await topicService.RenameAsync(id, request.Name, cancellationToken);
        return Ok(TopicResponse.FromApplication(topic));
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetStatus(
        int id,
        SetTopicStatusRequest request,
        CancellationToken cancellationToken)
    {
        await topicService.SetStatusAsync(id, request.IsActive, cancellationToken);
        return NoContent();
    }
}
