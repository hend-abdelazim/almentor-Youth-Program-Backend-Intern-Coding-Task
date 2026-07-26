using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.QueryParameters;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    private Guid CurrentUserId
    {
        get
        {
            var uidClaim = User.FindFirstValue("uid")
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue(ClaimTypes.SerialNumber)
                           ?? User.FindFirstValue("sub");

            if (uidClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return Guid.Parse(uidClaim);
        }
    }

    [HttpPost("projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromBody] CreateTaskRequestDto request,
        [FromServices] IValidator<CreateTaskRequestDto> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var response = await _taskService.CreateTaskForUserAsync(projectId, CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetTaskById), new { id = response.Id }, response);
    }

    [HttpGet("projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(PagedResponseDto<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProjectId(
        [FromRoute] Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] DateTime? due_date_from = null,
        [FromQuery] DateTime? due_date_to = null,
        [FromQuery] string? sort_by = null,
        [FromQuery] string? sort_direction = "asc",
        CancellationToken cancellationToken = default)
    {
        var queryParams = new TaskQueryParameters
        {
            Page = page < 1 ? 1 : page,
            Limit = limit < 1 ? 10 : limit > 100 ? 100 : limit,
            Status = status,
            Priority = priority,
            DueDateFrom = due_date_from,
            DueDateTo = due_date_to,
            SortBy = sort_by,
            SortDirection = sort_direction
        };

        var response = await _taskService.GetTasksByProjectIdForUserAsync(projectId, CurrentUserId, queryParams, cancellationToken);
        return Ok(response);
    }

    [HttpGet("tasks")]
    [ProducesResponseType(typeof(PagedResponseDto<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] DateTime? due_date_from = null,
        [FromQuery] DateTime? due_date_to = null,
        [FromQuery] string? sort_by = null,
        [FromQuery] string? sort_direction = "asc",
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new TaskQueryParameters
        {
            Page = page < 1 ? 1 : page,
            Limit = limit < 1 ? 10 : limit > 100 ? 100 : limit,
            Status = status,
            Priority = priority,
            DueDateFrom = due_date_from,
            DueDateTo = due_date_to,
            SortBy = sort_by,
            SortDirection = sort_direction,
            Search = q
        };

        var response = await _taskService.GetAllTasksForUserAsync(CurrentUserId, queryParams, cancellationToken);
        return Ok(response);
    }

    [HttpGet("tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _taskService.GetTaskByIdForUserAsync(id, CurrentUserId, cancellationToken);
        if (response == null)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Task with id '{id}' was not found.",
                Instance = Request.Path
            });

        return Ok(response);
    }

    [HttpPut("tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskRequestDto request,
        [FromServices] IValidator<UpdateTaskRequestDto> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var response = await _taskService.UpdateTaskForUserAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("tasks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteTaskForUserAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }
}
