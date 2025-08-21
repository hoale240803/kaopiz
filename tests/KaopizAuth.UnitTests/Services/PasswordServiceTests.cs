using FluentAssertions;
using KaopizAuth.Infrastructure.Services.Authentication;
using Xunit;

namespace KaopizAuth.UnitTests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_ValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var hashedPassword = _passwordService.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Should().StartWith("$2a$12$"); // BCrypt hash format
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void HashPassword_InvalidPassword_ShouldThrowArgumentNullException(string password)
    {
        // Act & Assert
        _passwordService.Invoking(x => x.HashPassword(password))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(hashedPassword, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "Password123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(hashedPassword, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void VerifyPassword_InvalidProvidedPassword_ShouldReturnFalse(string providedPassword)
    {
        // Arrange
        var hashedPassword = _passwordService.HashPassword("Password123!");

        // Act
        var result = _passwordService.VerifyPassword(hashedPassword, providedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void VerifyPassword_InvalidHashedPassword_ShouldThrowArgumentNullException(string hashedPassword)
    {
        // Act & Assert
        _passwordService.Invoking(x => x.VerifyPassword(hashedPassword, "Password123!"))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VerifyPassword_InvalidHashFormat_ShouldReturnFalse()
    {
        // Arrange
        var invalidHash = "invalid-hash-format";
        var password = "Password123!";

        // Act
        var result = _passwordService.VerifyPassword(invalidHash, password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ShouldGenerateDifferentHashes()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt uses random salt
        _passwordService.VerifyPassword(hash1, password).Should().BeTrue();
        _passwordService.VerifyPassword(hash2, password).Should().BeTrue();
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("Password123!")]
    [InlineData("VeryLongPasswordWith123!@#$%^&*()_+")]
    [InlineData("短密码123!")]
    public void HashAndVerify_VariousPasswords_ShouldWorkCorrectly(string password)
    {
        // Act
        var hashedPassword = _passwordService.HashPassword(password);
        var verificationResult = _passwordService.VerifyPassword(hashedPassword, password);

        // Assert
        verificationResult.Should().BeTrue();
    }
}
