using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.QueryParameters;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Project?> GetByIdWithOwnerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Project>> GetByOwnerIdAsync(
        Guid ownerId,
        ProjectQueryParameters queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .Where(p => p.OwnerId == ownerId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.Limit)
            .Take(queryParams.Limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>
        {
            Items = items,
            Page = queryParams.Page,
            Limit = queryParams.Limit,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.Limit)
        };
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Update(project);
        return await Task.FromResult(project);
    }

    public async Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(project);
        await Task.CompletedTask;
    }

    public async Task<bool> NameExistsForOwnerAsync(string name, Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AnyAsync(p => p.OwnerId == ownerId && p.Name == name, cancellationToken);
    }

    public async Task<bool> NameExistsForOwnerExcludingIdAsync(string name, Guid ownerId, Guid excludeId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AnyAsync(p => p.OwnerId == ownerId && p.Name == name && p.Id != excludeId, cancellationToken);
    }
}
