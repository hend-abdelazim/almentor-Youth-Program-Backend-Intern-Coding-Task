using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.Validators;

namespace TaskManagement.UnitTests.Validators;

public class ProjectValidationTests
{
    private readonly CreateProjectRequestValidator _createValidator;
    private readonly UpdateProjectRequestValidator _updateValidator;

    public ProjectValidationTests()
    {
        _createValidator = new CreateProjectRequestValidator();
        _updateValidator = new UpdateProjectRequestValidator();
    }

    [Fact]
    public async Task CreateProject_EmptyName_ShouldFail()
    {
        var request = new CreateProjectRequestDto { Name = "" };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task CreateProject_ValidName_ShouldPass()
    {
        var request = new CreateProjectRequestDto { Name = "My Project" };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task CreateProject_LongName_ShouldFail()
    {
        var request = new CreateProjectRequestDto { Name = new string('A', 201) };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task CreateProject_LongDescription_ShouldFail()
    {
        var request = new CreateProjectRequestDto { Name = "Test", Description = new string('A', 2001) };
        var result = await _createValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task UpdateProject_EmptyName_ShouldFail()
    {
        var request = new UpdateProjectRequestDto { Name = "" };
        var result = await _updateValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task UpdateProject_ValidRequest_ShouldPass()
    {
        var request = new UpdateProjectRequestDto { Name = "Updated Project", Description = "Some description" };
        var result = await _updateValidator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
