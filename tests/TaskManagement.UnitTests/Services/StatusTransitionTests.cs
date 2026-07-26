using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;
using TaskManagement.Application.DTOs.Tasks;

namespace TaskManagement.UnitTests.Services;

public class StatusTransitionTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly Mock<ITaskRepository> _mockTaskRepo;
    private readonly TaskService _taskService;

    public StatusTransitionTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<TaskService>>();
        _mockTaskRepo = new Mock<ITaskRepository>();

        _mockUnitOfWork.Setup(uow => uow.Tasks).Returns(_mockTaskRepo.Object);

        _taskService = new TaskService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateTask_TodoToInProgress_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var ownerId = userId;

        var task = new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            Title = "Test Task",
            Status = TaskStatus.Todo,
            Priority = TaskPriority.Medium,
            Project = new Project { Id = projectId, OwnerId = ownerId }
        };

        _mockTaskRepo.Setup(r => r.GetByIdWithProjectAndOwnerAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _mockTaskRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateTaskRequestDto
        {
            Title = "Test Task",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium
        };

        var result = await _taskService.UpdateTaskForUserAsync(taskId, userId, request);

        Assert.NotNull(result);
        Assert.Equal(TaskStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateTask_InProgressToDone_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            Title = "Test Task",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            Project = new Project { Id = projectId, OwnerId = userId }
        };

        _mockTaskRepo.Setup(r => r.GetByIdWithProjectAndOwnerAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _mockTaskRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateTaskRequestDto
        {
            Title = "Test Task",
            Status = TaskStatus.Done,
            Priority = TaskPriority.Medium
        };

        var result = await _taskService.UpdateTaskForUserAsync(taskId, userId, request);

        Assert.NotNull(result);
        Assert.Equal(TaskStatus.Done, result.Status);
    }

    [Fact]
    public async Task UpdateTask_DoneToTodo_ShouldBeAllowed_AndLoggedAsWarning()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            Title = "Test Task",
            Status = TaskStatus.Done,
            Priority = TaskPriority.Medium,
            Project = new Project { Id = projectId, OwnerId = userId }
        };

        _mockTaskRepo.Setup(r => r.GetByIdWithProjectAndOwnerAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _mockTaskRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateTaskRequestDto
        {
            Title = "Test Task",
            Status = TaskStatus.Todo,
            Priority = TaskPriority.Medium
        };

        var result = await _taskService.UpdateTaskForUserAsync(taskId, userId, request);

        Assert.NotNull(result);
        Assert.Equal(TaskStatus.Todo, result.Status);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unusual status transition") && v.ToString()!.Contains("done") && v.ToString()!.Contains("todo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTask_AnyStatusTransitionDirection_ShouldBeAllowed()
    {
        var transitions = new[]
        {
            new { From = TaskStatus.Todo, To = TaskStatus.InProgress },
            new { From = TaskStatus.InProgress, To = TaskStatus.Todo },
            new { From = TaskStatus.InProgress, To = TaskStatus.Done },
            new { From = TaskStatus.Done, To = TaskStatus.InProgress },
            new { From = TaskStatus.Todo, To = TaskStatus.Done },
            new { From = TaskStatus.Done, To = TaskStatus.Todo },
        };

        var userId = Guid.NewGuid();

        foreach (var transition in transitions)
        {
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var task = new TaskItem
            {
                Id = taskId,
                ProjectId = projectId,
                Title = "Test Task",
                Status = transition.From,
                Priority = TaskPriority.Medium,
                Project = new Project { Id = projectId, OwnerId = userId }
            };

            var mockTaskRepo = new Mock<ITaskRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<TaskService>>();

            mockUnitOfWork.Setup(uow => uow.Tasks).Returns(mockTaskRepo.Object);
            mockTaskRepo.Setup(r => r.GetByIdWithProjectAndOwnerAsync(taskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            mockTaskRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem t, CancellationToken _) => t);
            mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var service = new TaskService(mockUnitOfWork.Object, mockLogger.Object);

            var request = new UpdateTaskRequestDto
            {
                Title = "Test Task",
                Status = transition.To,
                Priority = TaskPriority.Medium
            };

            var result = await service.UpdateTaskForUserAsync(taskId, userId, request);

            Assert.NotNull(result);
            Assert.Equal(transition.To, result.Status);
        }
    }
}
