using FluentAssertions;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Services.Authentication;
using KaopizAuth.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace KaopizAuth.UnitTests.Services.Authentication;

public class UserAuthenticationStrategyFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly UserAuthenticationStrategyFactory _factory;

    public UserAuthenticationStrategyFactoryTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _factory = new UserAuthenticationStrategyFactory(_serviceProviderMock.Object);
    }

    [Fact]
    public void Constructor_NullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserAuthenticationStrategyFactory(null!));
    }

    [Fact]
    public void CreateStrategy_EndUserType_ShouldReturnEndUserAuthStrategy()
    {
        // Arrange
        var expectedStrategy = new Mock<EndUserAuthStrategy>(Mock.Of<IPasswordService>()).Object;
        _serviceProviderMock.Setup(x => x.GetService(typeof(EndUserAuthStrategy)))
            .Returns(expectedStrategy);

        // Act
        var result = _factory.CreateStrategy(UserType.EndUser);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(expectedStrategy);
        _serviceProviderMock.Verify(x => x.GetService(typeof(EndUserAuthStrategy)), Times.Once);
    }

    [Fact]
    public void CreateStrategy_AdminType_ShouldReturnAdminAuthStrategy()
    {
        // Arrange
        var expectedStrategy = new Mock<AdminAuthStrategy>(Mock.Of<IPasswordService>()).Object;
        _serviceProviderMock.Setup(x => x.GetService(typeof(AdminAuthStrategy)))
            .Returns(expectedStrategy);

        // Act
        var result = _factory.CreateStrategy(UserType.Admin);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(expectedStrategy);
        _serviceProviderMock.Verify(x => x.GetService(typeof(AdminAuthStrategy)), Times.Once);
    }

    [Fact]
    public void CreateStrategy_PartnerType_ShouldReturnPartnerAuthStrategy()
    {
        // Arrange
        var expectedStrategy = new Mock<PartnerAuthStrategy>(Mock.Of<IPasswordService>()).Object;
        _serviceProviderMock.Setup(x => x.GetService(typeof(PartnerAuthStrategy)))
            .Returns(expectedStrategy);

        // Act
        var result = _factory.CreateStrategy(UserType.Partner);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(expectedStrategy);
        _serviceProviderMock.Verify(x => x.GetService(typeof(PartnerAuthStrategy)), Times.Once);
    }

    [Fact]
    public void CreateStrategy_InvalidUserType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var invalidUserType = (UserType)999;

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => _factory.CreateStrategy(invalidUserType));
        exception.Message.Should().Contain($"User type '{invalidUserType}' is not supported");
    }

    [Fact]
    public void CreateStrategy_ServiceProviderReturnsNull_ShouldThrowException()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(EndUserAuthStrategy)))
            .Returns(null);

        // Act & Assert
        // This would throw when GetRequiredService is called (which throws if service is null)
        Assert.Throws<InvalidOperationException>(() => _factory.CreateStrategy(UserType.EndUser));
    }

    [Fact]
    public void CreateStrategy_AllUserTypes_ShouldMapCorrectly()
    {
        // Arrange
        var endUserStrategy = new Mock<EndUserAuthStrategy>(Mock.Of<IPasswordService>()).Object;
        var adminStrategy = new Mock<AdminAuthStrategy>(Mock.Of<IPasswordService>()).Object;
        var partnerStrategy = new Mock<PartnerAuthStrategy>(Mock.Of<IPasswordService>()).Object;

        _serviceProviderMock.Setup(x => x.GetService(typeof(EndUserAuthStrategy)))
            .Returns(endUserStrategy);
        _serviceProviderMock.Setup(x => x.GetService(typeof(AdminAuthStrategy)))
            .Returns(adminStrategy);
        _serviceProviderMock.Setup(x => x.GetService(typeof(PartnerAuthStrategy)))
            .Returns(partnerStrategy);

        // Act & Assert
        _factory.CreateStrategy(UserType.EndUser).Should().BeSameAs(endUserStrategy);
        _factory.CreateStrategy(UserType.Admin).Should().BeSameAs(adminStrategy);
        _factory.CreateStrategy(UserType.Partner).Should().BeSameAs(partnerStrategy);

        // Verify all services were requested
        _serviceProviderMock.Verify(x => x.GetService(typeof(EndUserAuthStrategy)), Times.Once);
        _serviceProviderMock.Verify(x => x.GetService(typeof(AdminAuthStrategy)), Times.Once);
        _serviceProviderMock.Verify(x => x.GetService(typeof(PartnerAuthStrategy)), Times.Once);
    }
}

// Integration test for the factory with real service provider
public class UserAuthenticationStrategyFactoryIntegrationTests
{
    [Fact]
    public void CreateStrategy_WithRealServiceProvider_ShouldWorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var passwordServiceMock = new Mock<IPasswordService>();
        
        services.AddSingleton(passwordServiceMock.Object);
        services.AddScoped<EndUserAuthStrategy>();
        services.AddScoped<AdminAuthStrategy>();
        services.AddScoped<PartnerAuthStrategy>();
        services.AddScoped<IUserAuthenticationStrategyFactory, UserAuthenticationStrategyFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();

        // Act & Assert
        var endUserStrategy = factory.CreateStrategy(UserType.EndUser);
        var adminStrategy = factory.CreateStrategy(UserType.Admin);
        var partnerStrategy = factory.CreateStrategy(UserType.Partner);

        endUserStrategy.Should().NotBeNull();
        endUserStrategy.Should().BeOfType<EndUserAuthStrategy>();

        adminStrategy.Should().NotBeNull();
        adminStrategy.Should().BeOfType<AdminAuthStrategy>();

        partnerStrategy.Should().NotBeNull();
        partnerStrategy.Should().BeOfType<PartnerAuthStrategy>();

        // Verify different instances are created (scoped)
        var endUserStrategy2 = factory.CreateStrategy(UserType.EndUser);
        // Note: Since we're using a single service provider scope, the same instance is returned
        // In ASP.NET Core, each request would have its own scope
        endUserStrategy2.Should().BeSameAs(endUserStrategy); // Same scope = same instance

        // Clean up
        serviceProvider.Dispose();
    }
}