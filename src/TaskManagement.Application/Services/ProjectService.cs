using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.QueryParameters;

namespace TaskManagement.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IUnitOfWork unitOfWork, ILogger<ProjectService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResponseDto<ProjectResponseDto>> GetProjectsForUserAsync(
        Guid userId,
        ProjectQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        var projects = await _unitOfWork.Projects.GetByOwnerIdAsync(userId, queryParams, cancellationToken);
        return projects.ToPagedResponseDto(p => p.ToProjectResponseDto());
    }

    public async Task<ProjectResponseDto?> GetProjectByIdForUserAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdWithOwnerAsync(projectId, cancellationToken);

        if (project == null)
            return null;

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to access this project.");

        return project.ToProjectResponseDto();
    }

    public async Task<ProjectResponseDto> CreateProjectForUserAsync(
        Guid userId,
        CreateProjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Projects.NameExistsForOwnerAsync(request.Name, userId, cancellationToken))
        {
            throw new DuplicateEntityException("Project", "Name", request.Name);
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Projects.CreateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToProjectResponseDto();
    }

    public async Task<ProjectResponseDto> UpdateProjectForUserAsync(
        Guid projectId,
        Guid userId,
        UpdateProjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdWithOwnerAsync(projectId, cancellationToken);

        if (project == null)
            throw new NotFoundException("Project", projectId);

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to update this project.");

        if (await _unitOfWork.Projects.NameExistsForOwnerExcludingIdAsync(request.Name, userId, projectId, cancellationToken))
        {
            throw new DuplicateEntityException("Project", "Name", request.Name);
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToProjectResponseDto();
    }

    public async Task DeleteProjectForUserAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdWithOwnerAsync(projectId, cancellationToken);

        if (project == null)
            throw new NotFoundException("Project", projectId);

        if (project.OwnerId != userId)
            throw new ForbiddenAccessException("You don't have permission to delete this project.");

        await _unitOfWork.Tasks.SoftDeleteByProjectIdAsync(projectId, cancellationToken);
        await _unitOfWork.Projects.DeleteAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} soft-deleted along with its tasks.", projectId);
    }
}
