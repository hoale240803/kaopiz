using FluentAssertions;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Services.Authentication;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Enums;
using Moq;
using System.Security.Claims;
using Xunit;

namespace KaopizAuth.UnitTests.Services.Authentication;

public class EndUserAuthStrategyTests
{
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly EndUserAuthStrategy _strategy;

    public EndUserAuthStrategyTests()
    {
        _passwordServiceMock = new Mock<IPasswordService>();
        _strategy = new EndUserAuthStrategy(_passwordServiceMock.Object);
    }

    [Fact]
    public void Constructor_NullPasswordService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EndUserAuthStrategy(null!));
    }

    [Fact]
    public async Task AuthenticateAsync_ValidActiveUserWithCorrectPassword_ShouldReturnSuccess()
    {
        // Arrange
        var user = CreateValidEndUser();
        var password = "Password123!";
        
        _passwordServiceMock.Setup(x => x.VerifyPassword(user.PasswordHash!, password))
            .Returns(true);

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.RequiresTwoFactor.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_ValidUserWithIncorrectPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidEndUser();
        var password = "WrongPassword";
        
        _passwordServiceMock.Setup(x => x.VerifyPassword(user.PasswordHash!, password))
            .Returns(false);

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_InactiveUser_ShouldReturnValidationFailure()
    {
        // Arrange
        var user = CreateValidEndUser();
        user.IsActive = false;
        var password = "Password123!";

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("User account is deactivated");
    }

    [Fact]
    public async Task AuthenticateAsync_UnconfirmedEmail_ShouldReturnValidationFailure()
    {
        // Arrange
        var user = CreateValidEndUser();
        user.EmailConfirmed = false;
        var password = "Password123!";

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Email address must be confirmed before login");
    }

    [Fact]
    public async Task GetUserClaimsAsync_ValidEndUser_ShouldReturnCorrectClaims()
    {
        // Arrange
        var user = CreateValidEndUser();

        // Act
        var claims = await _strategy.GetUserClaimsAsync(user);

        // Assert
        claims.Should().NotBeNull();
        var claimsList = claims.ToList();
        
        // Check basic identity claims
        claimsList.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == user.FirstName);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Surname && c.Value == user.LastName);
        claimsList.Should().Contain(c => c.Type == "UserType" && c.Value == UserType.EndUser.ToString());
        claimsList.Should().Contain(c => c.Type == "FullName" && c.Value == user.FullName);

        // Check end user permissions
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewProfile");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanUpdateProfile");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanChangePassword");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_ValidActiveUser_ShouldReturnSuccess()
    {
        // Arrange
        var user = CreateValidEndUser();

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_InactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidEndUser();
        user.IsActive = false;

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("User account is deactivated");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_UnconfirmedEmail_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidEndUser();
        user.EmailConfirmed = false;

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Email address must be confirmed before login");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_InactiveUserWithUnconfirmedEmail_ShouldReturnMultipleErrors()
    {
        // Arrange
        var user = CreateValidEndUser();
        user.IsActive = false;
        user.EmailConfirmed = false;

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("User account is deactivated");
        result.Errors.Should().Contain("Email address must be confirmed before login");
    }

    private ApplicationUser CreateValidEndUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
            UserType = UserType.EndUser,
            IsActive = true,
            PasswordHash = "$2a$12$hashedpassword"
        };
    }
}