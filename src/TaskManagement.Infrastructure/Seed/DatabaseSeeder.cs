using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Auth;
using TaskManagement.Infrastructure.Persistence;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, bool isDevelopment = false)
    {
        if (!isDevelopment)
            return;

        await context.Database.EnsureCreatedAsync();

        if (!await context.Users.AnyAsync())
        {
            var passwordHasher = new PasswordHasher();
            var demoUser = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Username = "demouser",
                Email = "demo@example.com",
                PasswordHash = passwordHasher.HashPassword("DemoPass123!"),
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(demoUser);

            var project1 = new Project
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Website Redesign",
                Description = "Complete redesign of the company website with modern UI/UX",
                OwnerId = demoUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var project2 = new Project
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Mobile App Development",
                Description = "Cross-platform mobile application for customer engagement",
                OwnerId = demoUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            };

            await context.Projects.AddRangeAsync(project1, project2);

            var tasks = new List<TaskItem>
            {
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    ProjectId = project1.Id,
                    Title = "Design Homepage Mockups",
                    Description = "Create high-fidelity mockups for the homepage layout",
                    Status = TaskStatus.Done,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    ProjectId = project1.Id,
                    Title = "Implement Backend API",
                    Description = "Build REST API endpoints for the frontend",
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    ProjectId = project1.Id,
                    Title = "Write Unit Tests",
                    Description = "Ensure backend code has proper test coverage",
                    Status = TaskStatus.Todo,
                    Priority = TaskPriority.Medium,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new()
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    ProjectId = project2.Id,
                    Title = "Setup Development Environment",
                    Description = "Configure React Native and project structure",
                    Status = TaskStatus.Done,
                    Priority = TaskPriority.High,
                    DueDate = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-13),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new()
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    ProjectId = project2.Id,
                    Title = "Login Screen UI",
                    Description = "Design and implement login screen with form validation",
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    DueDate = DateTime.UtcNow.AddDays(2),
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    ProjectId = project2.Id,
                    Title = "Push Notifications",
                    Description = "Integrate Firebase Cloud Messaging",
                    Status = TaskStatus.Todo,
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                }
            };

            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }
    }
}
