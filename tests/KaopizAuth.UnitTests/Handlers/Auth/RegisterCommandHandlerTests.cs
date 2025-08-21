using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Handlers.Auth;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KaopizAuth.UnitTests.Handlers.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
        
        _handler = new RegisterCommandHandler(_userManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnSuccessResult()
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

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User registered successfully");
        result.UserId.Should().NotBeEmpty();
        result.Errors.Should().BeEmpty();

        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u => 
                u.Email == command.Email.ToLowerInvariant() &&
                u.FirstName == command.FirstName.Trim() &&
                u.LastName == command.LastName.Trim() &&
                u.UserType == command.UserType &&
                u.IsActive == true
            ), 
            command.Password), Times.Once);
    }

    [Fact]
    public async Task Handle_FailedUserCreation_ShouldReturnFailureResult()
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

        var identityErrors = new[]
        {
            new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" },
            new IdentityError { Code = "WeakPassword", Description = "Password is too weak" }
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed");
        result.UserId.Should().BeEmpty();
        result.Errors.Should().ContainKey("DuplicateEmail");
        result.Errors.Should().ContainKey("WeakPassword");
        result.Errors["DuplicateEmail"].Should().Contain("Email already exists");
        result.Errors["WeakPassword"].Should().Contain("Password is too weak");
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldReturnFailureResult()
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

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An unexpected error occurred during registration");
        result.UserId.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(UserType.EndUser)]
    [InlineData(UserType.Admin)]
    [InlineData(UserType.Partner)]
    public async Task Handle_DifferentUserTypes_ShouldCreateUserWithCorrectType(UserType userType)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            UserType = userType
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u => u.UserType == userType), 
            command.Password), Times.Once);
    }
}
