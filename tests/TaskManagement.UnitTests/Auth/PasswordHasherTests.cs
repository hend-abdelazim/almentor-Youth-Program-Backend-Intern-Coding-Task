using TaskManagement.Infrastructure.Auth;

namespace TaskManagement.UnitTests.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword()
    {
        var password = "MySecurePassword123!";
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "MySecurePassword123!";
        var hash = _hasher.HashPassword(password);

        Assert.True(_hasher.VerifyPassword(password, hash));
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "MySecurePassword123!";
        var hash = _hasher.HashPassword(password);

        Assert.False(_hasher.VerifyPassword("WrongPassword!", hash));
    }

    [Fact]
    public void HashPassword_ShouldNotStorePlaintext()
    {
        var password = "MySecurePassword123!";
        var hash = _hasher.HashPassword(password);

        Assert.DoesNotContain(password, hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("VeryLongPassword1234567890!@#$%^&*()")]
    [InlineData("Password with spaces")]
    [InlineData("!@#$%^&*()")]
    public void HashAndVerify_ShouldWorkForVariousPasswords(string password)
    {
        var hash = _hasher.HashPassword(password);
        Assert.True(_hasher.VerifyPassword(password, hash));
    }
}
