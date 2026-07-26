using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Validators;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.UnitTests.Validators;

public class PriorityValidationTests
{
    private readonly CreateTaskRequestValidator _createValidator;
    private readonly UpdateTaskRequestValidator _updateValidator;

    public PriorityValidationTests()
    {
        _createValidator = new CreateTaskRequestValidator();
        _updateValidator = new UpdateTaskRequestValidator();
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    public async Task CreateTask_ValidPriorities_ShouldPass(TaskPriority priority)
    {
        var request = new CreateTaskRequestDto { Title = "Test", Priority = priority };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    public async Task UpdateTask_ValidPriorities_ShouldPass(TaskPriority priority)
    {
        var request = new UpdateTaskRequestDto
        {
            Title = "Test",
            Status = TaskStatus.Todo,
            Priority = priority
        };
        var result = await _updateValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }
}
