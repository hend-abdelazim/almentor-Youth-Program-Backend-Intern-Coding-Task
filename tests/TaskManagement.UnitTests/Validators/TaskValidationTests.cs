using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Validators;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.UnitTests.Validators;

public class TaskValidationTests
{
    private readonly CreateTaskRequestValidator _createValidator;
    private readonly UpdateTaskRequestValidator _updateValidator;

    public TaskValidationTests()
    {
        _createValidator = new CreateTaskRequestValidator();
        _updateValidator = new UpdateTaskRequestValidator();
    }

    [Fact]
    public async Task CreateTask_EmptyTitle_ShouldFail()
    {
        var request = new CreateTaskRequestDto { Title = "" };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task CreateTask_LongTitle_ShouldFail()
    {
        var request = new CreateTaskRequestDto { Title = new string('A', 501) };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task CreateTask_ValidTitle_ShouldPass()
    {
        var request = new CreateTaskRequestDto { Title = "Valid Task Title" };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }
}
