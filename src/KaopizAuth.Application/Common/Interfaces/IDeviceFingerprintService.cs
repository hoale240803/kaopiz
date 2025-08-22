namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Service interface for generating device fingerprints for security purposes
/// </summary>
public interface IDeviceFingerprintService
{
    /// <summary>
    /// Generates a device fingerprint based on IP address and user agent
    /// </summary>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent string</param>
    /// <returns>Device fingerprint hash</returns>
    string GenerateFingerprint(string? ipAddress, string? userAgent);

    /// <summary>
    /// Validates if a fingerprint matches the current request context
    /// </summary>
    /// <param name="storedFingerprint">Previously stored fingerprint</param>
    /// <param name="currentIpAddress">Current IP address</param>
    /// <param name="currentUserAgent">Current user agent</param>
    /// <returns>True if fingerprint matches, false if suspicious</returns>
    bool ValidateFingerprint(string? storedFingerprint, string? currentIpAddress, string? currentUserAgent);
}