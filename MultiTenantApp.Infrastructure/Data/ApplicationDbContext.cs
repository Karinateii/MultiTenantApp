using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly string? _tenantId;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, string? tenantId = null)
        : base(options)
    {
        _tenantId = tenantId;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantUser> TenantUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ConnectionString).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // TenantUser configuration
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
        });
    }
}
