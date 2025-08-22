using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KaopizAuth.Application.Services.Authentication;

/// <summary>
/// Factory interface for creating user authentication strategies
/// </summary>
public interface IUserAuthenticationStrategyFactory
{
    /// <summary>
    /// Creates the appropriate authentication strategy based on user type
    /// </summary>
    /// <param name="userType">The type of user</param>
    /// <returns>Authentication strategy instance</returns>
    IUserAuthenticationStrategy CreateStrategy(UserType userType);
}

/// <summary>
/// Factory implementation for creating user authentication strategies
/// </summary>
public class UserAuthenticationStrategyFactory : IUserAuthenticationStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UserAuthenticationStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Creates the appropriate authentication strategy based on user type
    /// </summary>
    /// <param name="userType">The type of user</param>
    /// <returns>Authentication strategy instance</returns>
    /// <exception cref="NotSupportedException">Thrown when user type is not supported</exception>
    public IUserAuthenticationStrategy CreateStrategy(UserType userType)
    {
        return userType switch
        {
            UserType.EndUser => _serviceProvider.GetRequiredService<EndUserAuthStrategy>(),
            UserType.Admin => _serviceProvider.GetRequiredService<AdminAuthStrategy>(),
            UserType.Partner => _serviceProvider.GetRequiredService<PartnerAuthStrategy>(),
            _ => throw new NotSupportedException($"User type '{userType}' is not supported")
        };
    }
}