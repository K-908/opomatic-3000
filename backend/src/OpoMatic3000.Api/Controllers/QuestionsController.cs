using Microsoft.AspNetCore.Mvc;
using OpoMatic3000.Api.Contracts.Questions;
using OpoMatic3000.Application.Questions;

namespace OpoMatic3000.Api.Controllers;

[ApiController]
[Route("api/questions")]
[Produces("application/json")]
public sealed class QuestionsController(QuestionService questionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<QuestionListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<QuestionListItemResponse>>> List(
        [FromQuery] int? topicId,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await questionService.ListAsync(
            topicId, includeInactive, search, page, pageSize, cancellationToken);
        return Ok(new PagedResponse<QuestionListItemResponse>(
            result.Items.Select(QuestionListItemResponse.FromApplication).ToArray(),
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalPages));
    }

    [HttpGet("{id:int}", Name = nameof(GetQuestion))]
    [ProducesResponseType<QuestionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionResponse>> GetQuestion(
        int id,
        CancellationToken cancellationToken)
    {
        var question = await questionService.GetAsync(id, cancellationToken);
        return Ok(QuestionResponse.FromApplication(question));
    }

    [HttpPost]
    [ProducesResponseType<QuestionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuestionResponse>> Create(
        SaveQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var question = await questionService.CreateAsync(
            request.TopicId,
            request.Statement,
            request.Options?.Select(option => option.ToApplication()).ToArray(),
            cancellationToken);
        var response = QuestionResponse.FromApplication(question);
        return CreatedAtRoute(nameof(GetQuestion), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<QuestionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuestionResponse>> Update(
        int id,
        SaveQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var question = await questionService.UpdateAsync(
            id,
            request.TopicId,
            request.Statement,
            request.Options?.Select(option => option.ToApplication()).ToArray(),
            cancellationToken);
        return Ok(QuestionResponse.FromApplication(question));
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetStatus(
        int id,
        SetQuestionStatusRequest request,
        CancellationToken cancellationToken)
    {
        await questionService.SetStatusAsync(id, request.IsActive, cancellationToken);
        return NoContent();
    }
}
