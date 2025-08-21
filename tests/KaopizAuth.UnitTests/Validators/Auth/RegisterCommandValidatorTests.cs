using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Validators.Auth;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KaopizAuth.UnitTests.Validators.Auth;

public class RegisterCommandValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<DbSet<ApplicationUser>> _userDbSetMock;
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userDbSetMock = new Mock<DbSet<ApplicationUser>>();
        _contextMock.Setup(x => x.Users).Returns(_userDbSetMock.Object);
        
        _validator = new RegisterCommandValidator(_contextMock.Object);
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserType.EndUser
        };

        _userDbSetMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyEmail_ShouldFail(string email)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Email = email };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public async Task Validate_InvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Email = email };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email) && e.ErrorMessage.Contains("Email format is invalid"));
    }

    [Fact]
    public async Task Validate_DuplicateEmail_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();

        _userDbSetMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email) && e.ErrorMessage.Contains("Email already exists"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("12345678")] // No uppercase
    [InlineData("PASSWORD123!")] // No lowercase
    [InlineData("Password!")] // No digit
    [InlineData("Password123")] // No special character
    public async Task Validate_InvalidPassword_ShouldFail(string password)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Password = password, ConfirmPassword = password };

        _userDbSetMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public async Task Validate_PasswordMismatch_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { ConfirmPassword = "DifferentPassword123!" };

        _userDbSetMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.ConfirmPassword) && e.ErrorMessage.Contains("Passwords do not match"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyFirstName_ShouldFail(string firstName)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { FirstName = firstName };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyLastName_ShouldFail(string lastName)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { LastName = lastName };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.LastName));
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("John@Doe")]
    [InlineData("John#Smith")]
    public async Task Validate_InvalidNameCharacters_ShouldFail(string name)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { FirstName = name };

        _userDbSetMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.FirstName));
    }

    [Theory]
    [InlineData(UserType.EndUser)]
    [InlineData(UserType.Admin)]
    [InlineData(UserType.Partner)]
    public async Task Validate_ValidUserTypes_ShouldPass(UserType userType)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { UserType = userType };

        _userDbSetMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static RegisterCommand CreateValidCommand()
    {
        return new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserType.EndUser
        };
    }
}
