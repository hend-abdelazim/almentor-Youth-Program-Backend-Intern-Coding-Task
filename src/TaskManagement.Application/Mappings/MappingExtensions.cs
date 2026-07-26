using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.QueryParameters;

namespace TaskManagement.Application.Mappings;

public static class MappingExtensions
{
    public static ProjectResponseDto ToProjectResponseDto(this Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    public static TaskResponseDto ToTaskResponseDto(this TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ProjectName = task.Project?.Name,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    public static PagedResponseDto<TResponse> ToPagedResponseDto<TSource, TResponse>(
        this PagedResult<TSource> pagedResult,
        Func<TSource, TResponse> mapper)
    {
        return new PagedResponseDto<TResponse>
        {
            Items = pagedResult.Items.Select(mapper).ToList(),
            Page = pagedResult.Page,
            Limit = pagedResult.Limit,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };
    }
}
