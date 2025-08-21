using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Infrastructure.Data;

namespace KaopizAuth.Infrastructure.Data.Repositories;

/// <summary>
/// Simplified Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // For now, implement a simple Users repository that throws NotImplementedException
    // This will be implemented in the User management tickets
    public IUserRepository Users => throw new NotImplementedException("User repository will be implemented in ticket 6");

    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // For now, return a simple transaction implementation
        // This can be enhanced later when we need full transaction support
        return new SimpleTransaction();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

/// <summary>
/// Simple transaction implementation
/// </summary>
public class SimpleTransaction : ITransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        // Simple implementation - in a real scenario, this would commit the transaction
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // Simple implementation - in a real scenario, this would rollback the transaction
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Nothing to dispose in this simple implementation
    }
}
