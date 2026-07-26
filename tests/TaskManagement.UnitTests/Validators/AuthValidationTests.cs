using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Validators;

namespace TaskManagement.UnitTests.Validators;

public class AuthValidationTests
{
    private readonly RegisterRequestValidator _registerValidator;
    private readonly LoginRequestValidator _loginValidator;

    public AuthValidationTests()
    {
        _registerValidator = new RegisterRequestValidator();
        _loginValidator = new LoginRequestValidator();
    }

    [Fact]
    public async Task Register_ValidRequest_ShouldPass()
    {
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var result = await _registerValidator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Register_ShortUsername_ShouldFail()
    {
        var request = new RegisterRequestDto
        {
            Username = "ab",
            Email = "test@example.com",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var result = await _registerValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public async Task Register_InvalidEmail_ShouldFail()
    {
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var result = await _registerValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Register_PasswordTooShort_ShouldFail()
    {
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "123",
            ConfirmPassword = "123"
        };
        var result = await _registerValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Register_PasswordsDoNotMatch_ShouldFail()
    {
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Pass123!",
            ConfirmPassword = "Pass321!"
        };
        var result = await _registerValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Password and confirm password do not match.");
    }

    [Fact]
    public async Task Login_EmptyCredentials_ShouldFail()
    {
        var request = new LoginRequestDto { UsernameOrEmail = "", Password = "" };
        var result = await _loginValidator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.UsernameOrEmail);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldPass()
    {
        var request = new LoginRequestDto { UsernameOrEmail = "test@example.com", Password = "Pass123!" };
        var result = await _loginValidator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
