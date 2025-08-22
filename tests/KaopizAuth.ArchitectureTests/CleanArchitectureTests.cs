using NetArchTest.Rules;
using FluentAssertions;

namespace KaopizAuth.ArchitectureTests;

public class CleanArchitectureTests
{
    private const string DomainNamespace = "KaopizAuth.Domain";
    private const string ApplicationNamespace = "KaopizAuth.Application";
    private const string InfrastructureNamespace = "KaopizAuth.Infrastructure";
    private const string WebApiNamespace = "KaopizAuth.WebAPI";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Domain.Entities.User).Assembly)
            .Should()
            .NotHaveDependencyOn(ApplicationNamespace)
            .And().NotHaveDependencyOn(InfrastructureNamespace)
            .And().NotHaveDependencyOn(WebApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on other layers. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructureOrWebApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Application.ApplicationServiceCollectionExtensions).Assembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .And().NotHaveDependencyOn(WebApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on Infrastructure or WebAPI layers. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOnWebApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Infrastructure.InfrastructureServiceCollectionExtensions).Assembly)
            .Should()
            .NotHaveDependencyOn(WebApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure layer should not depend on WebAPI layer. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Controllers_Should_HaveDependencyOnlyOnApplicationAndDomain()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That().ResideInNamespace($"{WebApiNamespace}.Controllers")
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Controllers should not directly depend on Infrastructure layer. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Entities_Should_BeSealed()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Domain.Entities.User).Assembly)
            .That().ResideInNamespace($"{DomainNamespace}.Entities")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain entities should be sealed. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ValueObjects_Should_BeSealed()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Domain.ValueObjects.Email).Assembly)
            .That().ResideInNamespace($"{DomainNamespace}.ValueObjects")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Value objects should be sealed. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommandHandlers_Should_BeSealed_And_Have_Handler_Suffix()
    {
        // Arrange & Act
        var handlerResult = Types.InAssembly(typeof(KaopizAuth.Application.ApplicationServiceCollectionExtensions).Assembly)
            .That().ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .BeSealed()
            .And().HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        handlerResult.IsSuccessful.Should().BeTrue(
            $"Command/Query handlers should be sealed and have 'Handler' suffix. Failing types: {string.Join(", ", handlerResult.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Repositories_Should_HaveOnlyInterfacesInDomain()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Domain.Entities.User).Assembly)
            .That().HaveNameEndingWith("Repository")
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Repository types in Domain should be interfaces only. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainServices_Should_BeInterfaces()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(KaopizAuth.Domain.Entities.User).Assembly)
            .That().ResideInNamespace($"{DomainNamespace}.Services")
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain services should be interfaces. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}