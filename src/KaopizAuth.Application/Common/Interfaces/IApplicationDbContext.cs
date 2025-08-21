using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// </summary>
public interface IApplicationDbContext
{
    // TODO: Add DbSets when Domain entities are implemented
    // DbSet<ApplicationUser> Users { get; }
    // DbSet<ApplicationRole> Roles { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}