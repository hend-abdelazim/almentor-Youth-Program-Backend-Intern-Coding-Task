using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
