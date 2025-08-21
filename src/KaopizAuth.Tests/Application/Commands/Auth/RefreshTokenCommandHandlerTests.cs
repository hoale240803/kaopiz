using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Commands.Auth.Handlers;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KaopizAuth.Tests.Application.Commands.Auth;

/// <summary>
/// Unit tests for RefreshTokenCommandHandler
/// </summary>
public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IRefreshTokenDomainService> _mockRefreshTokenDomainService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _mockLogger;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockRefreshTokenDomainService = new Mock<IRefreshTokenDomainService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockUserManager = MockUserManager();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RefreshTokenCommandHandler>>();

        _handler = new RefreshTokenCommandHandler(
            _mockRefreshTokenRepository.Object,
            _mockRefreshTokenDomainService.Object,
            _mockJwtTokenService.Object,
            _mockUserManager.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsSuccessWithNewTokens()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token",
            IpAddress = "127.0.0.1"
        };

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            IsActive = true
        };

        var existingRefreshToken = RefreshToken.Create(
            "valid-refresh-token",
            DateTime.UtcNow.AddDays(1),
            userId.ToString(),
            "127.0.0.1");

        var newRefreshToken = RefreshToken.Create(
            "new-refresh-token",
            DateTime.UtcNow.AddDays(1),
            userId.ToString(),
            "127.0.0.1");

        _mockRefreshTokenRepository
            .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRefreshToken);

        _mockRefreshTokenDomainService
            .Setup(x => x.CanUseRefreshToken(existingRefreshToken))
            .Returns(true);

        _mockUserManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _mockJwtTokenService
            .Setup(x => x.GenerateAccessTokenAsync(user))
            .ReturnsAsync("new-access-token");

        _mockRefreshTokenDomainService
            .Setup(x => x.GenerateRefreshToken(userId, command.IpAddress!))
            .Returns(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("new-access-token", result.Data.AccessToken);
        Assert.Equal("new-refresh-token", result.Data.RefreshToken);

        _mockRefreshTokenDomainService.Verify(
            x => x.RevokeRefreshToken(existingRefreshToken, command.IpAddress!, "Token used for refresh"),
            Times.Once);

        _mockRefreshTokenRepository.Verify(
            x => x.AddAsync(newRefreshToken, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "invalid-refresh-token",
            IpAddress = "127.0.0.1"
        };

        _mockRefreshTokenRepository
            .Setup(x => x.GetByTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid refresh token", result.Message);
    }

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }
}
