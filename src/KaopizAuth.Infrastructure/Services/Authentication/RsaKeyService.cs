using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace KaopizAuth.Infrastructure.Services.Authentication;

/// <summary>
/// RSA key provider service for JWT RS256 implementation
/// </summary>
public interface IRsaKeyService
{
    RsaSecurityKey GetRsaSecurityKey();
}

/// <summary>
/// RSA key provider service implementation
/// </summary>
public class RsaKeyService : IRsaKeyService, IDisposable
{
    private readonly RSA _rsa;
    private readonly RsaSecurityKey _rsaSecurityKey;

    public RsaKeyService()
    {
        _rsa = RSA.Create(2048);
        _rsaSecurityKey = new RsaSecurityKey(_rsa);
    }

    public RsaSecurityKey GetRsaSecurityKey()
    {
        return _rsaSecurityKey;
    }

    public void Dispose()
    {
        _rsa?.Dispose();
        GC.SuppressFinalize(this);
    }
}