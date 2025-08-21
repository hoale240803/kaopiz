using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public new DbSet<ApplicationRole> Roles => Set<ApplicationRole>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Configure Identity table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Add audit trail logic here when implementing user management (Ticket 6)
        return await base.SaveChangesAsync(cancellationToken);
    }
}