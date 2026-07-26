using TaskManagement.Domain.Entities;
using TaskManagement.Domain.QueryParameters;

namespace TaskManagement.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdWithOwnerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Project>> GetByOwnerIdAsync(Guid ownerId, ProjectQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
    Task<bool> NameExistsForOwnerAsync(string name, Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsForOwnerExcludingIdAsync(string name, Guid ownerId, Guid excludeId, CancellationToken cancellationToken = default);
}
