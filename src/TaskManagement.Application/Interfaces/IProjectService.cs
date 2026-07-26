using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Domain.QueryParameters;

namespace TaskManagement.Application.Interfaces;

public interface IProjectService
{
    Task<PagedResponseDto<ProjectResponseDto>> GetProjectsForUserAsync(Guid userId, ProjectQueryParameters queryParams, CancellationToken cancellationToken = default);
    Task<ProjectResponseDto?> GetProjectByIdForUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectResponseDto> CreateProjectForUserAsync(Guid userId, CreateProjectRequestDto request, CancellationToken cancellationToken = default);
    Task<ProjectResponseDto> UpdateProjectForUserAsync(Guid projectId, Guid userId, UpdateProjectRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteProjectForUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
