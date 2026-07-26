using TaskManagement.Domain.Entities;
using TaskManagement.Domain.QueryParameters;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdWithProjectAndOwnerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskItem>> GetByProjectIdAsync(Guid projectId, TaskQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskItem>> GetAllByOwnerIdAsync(Guid ownerId, TaskQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task SoftDeleteByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}
