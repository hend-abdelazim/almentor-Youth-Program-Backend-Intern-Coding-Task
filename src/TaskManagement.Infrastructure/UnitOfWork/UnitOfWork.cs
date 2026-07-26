using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users
    {
        get
        {
            _users ??= new UserRepository(_context);
            return _users;
        }
    }

    public IProjectRepository Projects
    {
        get
        {
            _projects ??= new ProjectRepository(_context);
            return _projects;
        }
    }

    public ITaskRepository Tasks
    {
        get
        {
            _tasks ??= new TaskRepository(_context);
            return _tasks;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
