using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.QueryParameters;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<TaskItem?> GetByIdWithProjectAndOwnerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.Project)
                .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<PagedResult<TaskItem>> GetByProjectIdAsync(
        Guid projectId,
        TaskQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Where(t => t.ProjectId == projectId)
            .AsQueryable();

        query = ApplyFilters(query, queryParams);
        query = ApplySorting(query, queryParams);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.Limit)
            .Take(queryParams.Limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            Page = queryParams.Page,
            Limit = queryParams.Limit,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.Limit)
        };
    }

    public async Task<PagedResult<TaskItem>> GetAllByOwnerIdAsync(
        Guid ownerId,
        TaskQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Where(t => t.Project.OwnerId == ownerId)
            .AsQueryable();

        query = ApplyFilters(query, queryParams);
        query = ApplySearch(query, queryParams);
        query = ApplySorting(query, queryParams);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.Limit)
            .Take(queryParams.Limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            Page = queryParams.Page,
            Limit = queryParams.Limit,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.Limit)
        };
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        return await Task.FromResult(task);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        task.DeletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await Task.CompletedTask;
    }

    public async Task SoftDeleteByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            task.DeletedAt = now;
            task.UpdatedAt = now;
        }

        _context.Tasks.UpdateRange(tasks);
    }

    private static IQueryable<TaskItem> ApplyFilters(IQueryable<TaskItem> query, TaskQueryParameters queryParams)
    {
        if (queryParams.Status.HasValue)
        {
            query = query.Where(t => t.Status == queryParams.Status.Value);
        }

        if (queryParams.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == queryParams.Priority.Value);
        }

        if (queryParams.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= queryParams.DueDateFrom.Value);
        }

        if (queryParams.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= queryParams.DueDateTo.Value);
        }

        return query;
    }

    private static IQueryable<TaskItem> ApplySearch(IQueryable<TaskItem> query, TaskQueryParameters queryParams)
    {
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchTerm = queryParams.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(searchTerm) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        return query;
    }

    private static IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, TaskQueryParameters queryParams)
    {
        if (string.IsNullOrWhiteSpace(queryParams.SortBy))
        {
            return query.OrderByDescending(t => t.CreatedAt);
        }

        bool isDescending = queryParams.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        Expression<Func<TaskItem, object?>> keySelector = queryParams.SortBy.ToLower() switch
        {
            "due_date" => t => t.DueDate,
            "priority" => t => t.Priority,
            "created_at" => t => t.CreatedAt,
            _ => t => t.CreatedAt
        };

        return isDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
