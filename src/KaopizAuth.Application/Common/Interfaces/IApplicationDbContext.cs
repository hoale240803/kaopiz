using KaopizAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// </summary>
public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}