using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.QueryParameters;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResponseDto<TaskResponseDto>> GetTasksByProjectIdForUserAsync(Guid projectId, Guid userId, TaskQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<PagedResponseDto<TaskResponseDto>> GetAllTasksForUserAsync(Guid userId, TaskQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<TaskResponseDto?> GetTaskByIdForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> CreateTaskForUserAsync(Guid projectId, Guid userId, CreateTaskRequestDto request, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> UpdateTaskForUserAsync(Guid taskId, Guid userId, UpdateTaskRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteTaskForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}
