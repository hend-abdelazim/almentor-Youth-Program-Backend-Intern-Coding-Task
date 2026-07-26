using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Validators;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.UnitTests.Validators;

public class StatusValidationTests
{
    private readonly CreateTaskRequestValidator _createValidator;
    private readonly UpdateTaskRequestValidator _updateValidator;

    public StatusValidationTests()
    {
        _createValidator = new CreateTaskRequestValidator();
        _updateValidator = new UpdateTaskRequestValidator();
    }

    [Theory]
    [InlineData(TaskStatus.Todo)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Done)]
    public async Task CreateTask_ValidStatuses_ShouldPass(TaskStatus status)
    {
        var request = new CreateTaskRequestDto { Title = "Test", Status = status };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(TaskStatus.Todo)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Done)]
    public async Task UpdateTask_ValidStatuses_ShouldPass(TaskStatus status)
    {
        var request = new UpdateTaskRequestDto
        {
            Title = "Test",
            Status = status,
            Priority = TaskPriority.Medium
        };
        var result = await _updateValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public async Task CreateTask_InvalidStatus_ShouldFail()
    {
        var request = new CreateTaskRequestDto { Title = "Test" };
        var invalidStatus = (TaskStatus)999;
        request.GetType().GetProperty("Status")!.SetValue(request, invalidStatus);

        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
