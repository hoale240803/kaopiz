using FluentValidation;
using KaopizAuth.Application.Commands.Auth;

namespace KaopizAuth.Tests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsNoErrors()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyEmail_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "",
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Email is required");
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "invalid-email",
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Invalid email format");
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password is required");
    }

    [Fact]
    public void Validate_ShortPassword_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Password must be at least 8 characters long");
    }
}