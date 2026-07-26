using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.QueryParameters;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;

    public TaskService(IUnitOfWork unitOfWork, ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResponseDto<TaskResponseDto>> GetTasksByProjectIdForUserAsync(
        Guid projectId,
        Guid userId,
        TaskQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdWithOwnerAsync(projectId, cancellationToken);

        if (project == null)
            throw new NotFoundException("Project", projectId);

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to access tasks in this project.");

        var tasks = await _unitOfWork.Tasks.GetByProjectIdAsync(projectId, queryParams, cancellationToken);
        return tasks.ToPagedResponseDto(t => t.ToTaskResponseDto());
    }

    public async Task<PagedResponseDto<TaskResponseDto>> GetAllTasksForUserAsync(
        Guid userId,
        TaskQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllByOwnerIdAsync(userId, queryParams, cancellationToken);
        return tasks.ToPagedResponseDto(t => t.ToTaskResponseDto());
    }

    public async Task<TaskResponseDto?> GetTaskByIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithProjectAndOwnerAsync(taskId, cancellationToken);

        if (task == null)
            return null;

        if (task.Project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to access this task.");

        return task.ToTaskResponseDto();
    }

    public async Task<TaskResponseDto> CreateTaskForUserAsync(
        Guid projectId,
        Guid userId,
        CreateTaskRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdWithOwnerAsync(projectId, cancellationToken);

        if (project == null)
            throw new NotFoundException("Project", projectId);

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to create tasks in this project.");

        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            throw new ValidationException("Due date cannot be in the past.");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status ?? TaskStatus.Todo,
            Priority = request.Priority ?? TaskPriority.Medium,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tasks.CreateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        task.Project = project;
        return task.ToTaskResponseDto();
    }

    public async Task<TaskResponseDto> UpdateTaskForUserAsync(
        Guid taskId,
        Guid userId,
        UpdateTaskRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithProjectAndOwnerAsync(taskId, cancellationToken);

        if (task == null)
            throw new NotFoundException("Task", taskId);

        if (task.Project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to update this task.");

        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            throw new ValidationException("Due date cannot be in the past.");
        }

        var oldStatus = task.Status;
        var newStatus = request.Status;

        if (oldStatus == TaskStatus.Done && newStatus == TaskStatus.Todo)
        {
            _logger.LogWarning("Unusual status transition detected: Task {TaskId} transitioning from 'done' to 'todo'.", taskId);
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = newStatus;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToTaskResponseDto();
    }

    public async Task DeleteTaskForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithProjectAndOwnerAsync(taskId, cancellationToken);

        if (task == null)
            throw new NotFoundException("Task", taskId);

        if (task.Project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to delete this task.");

        await _unitOfWork.Tasks.DeleteAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
