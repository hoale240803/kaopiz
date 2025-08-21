using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for RefreshToken operations
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken, Guid>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId.ToString())
            .OrderByDescending(rt => rt.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId.ToString() &&
                        !rt.RevokedAt.HasValue &&
                        rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow &&
                        !rt.RevokedAt.HasValue)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> RevokeAllUserTokensAsync(Guid userId, string ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId, cancellationToken);
        var revokedCount = 0;

        foreach (var token in activeTokens)
        {
            token.Revoke(ipAddress, reason, null, "System");
            Update(token); // Use the Update method from base class
            revokedCount++;
        }

        return revokedCount;
    }

    public async Task<int> RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow.AddDays(-30)) // Remove tokens expired for more than 30 days
            .ToListAsync(cancellationToken);

        foreach (var token in expiredTokens)
        {
            token.Revoke("System", "Expired token cleanup");
            Update(token);
        }

        return expiredTokens.Count;
    }
}
