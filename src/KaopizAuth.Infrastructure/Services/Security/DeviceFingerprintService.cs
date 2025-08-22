using KaopizAuth.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace KaopizAuth.Infrastructure.Services.Security;

/// <summary>
/// Implementation of device fingerprinting service for security purposes
/// </summary>
public class DeviceFingerprintService : IDeviceFingerprintService
{
    /// <summary>
    /// Generates a device fingerprint based on IP address and user agent
    /// </summary>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent string</param>
    /// <returns>Device fingerprint hash</returns>
    public string GenerateFingerprint(string? ipAddress, string? userAgent)
    {
        var fingerprintData = $"{ipAddress ?? "unknown"}|{userAgent ?? "unknown"}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprintData));
        
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Validates if a fingerprint matches the current request context
    /// </summary>
    /// <param name="storedFingerprint">Previously stored fingerprint</param>
    /// <param name="currentIpAddress">Current IP address</param>
    /// <param name="currentUserAgent">Current user agent</param>
    /// <returns>True if fingerprint matches, false if suspicious</returns>
    public bool ValidateFingerprint(string? storedFingerprint, string? currentIpAddress, string? currentUserAgent)
    {
        if (string.IsNullOrEmpty(storedFingerprint))
            return false;

        var currentFingerprint = GenerateFingerprint(currentIpAddress, currentUserAgent);
        return string.Equals(storedFingerprint, currentFingerprint, StringComparison.Ordinal);
    }
}