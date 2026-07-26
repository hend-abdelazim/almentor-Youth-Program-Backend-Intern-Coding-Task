using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Validators;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.UnitTests.Validators;

public class DueDateValidationTests
{
    private readonly CreateTaskRequestValidator _createValidator;
    private readonly UpdateTaskRequestValidator _updateValidator;

    public DueDateValidationTests()
    {
        _createValidator = new CreateTaskRequestValidator();
        _updateValidator = new UpdateTaskRequestValidator();
    }

    [Fact]
    public async Task CreateTask_NullDueDate_ShouldPass()
    {
        var request = new CreateTaskRequestDto { Title = "Test", DueDate = null };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task CreateTask_TodayDueDate_ShouldPass()
    {
        var request = new CreateTaskRequestDto { Title = "Test", DueDate = DateTime.UtcNow.Date };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task CreateTask_FutureDueDate_ShouldPass()
    {
        var request = new CreateTaskRequestDto { Title = "Test", DueDate = DateTime.UtcNow.AddDays(5) };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task CreateTask_PastDueDate_ShouldFail()
    {
        var request = new CreateTaskRequestDto { Title = "Test", DueDate = DateTime.UtcNow.AddDays(-1) };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date cannot be in the past.");
    }

    [Fact]
    public async Task UpdateTask_PastDueDate_ShouldFail()
    {
        var request = new UpdateTaskRequestDto
        {
            Title = "Test",
            Status = TaskStatus.Todo,
            Priority = Domain.Enums.TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(-2)
        };
        var result = await _updateValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async Task UpdateTask_TodayDueDate_ShouldPass()
    {
        var request = new UpdateTaskRequestDto
        {
            Title = "Test",
            Status = TaskStatus.Todo,
            Priority = Domain.Enums.TaskPriority.Medium,
            DueDate = DateTime.UtcNow.Date
        };
        var result = await _updateValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }
}
