using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
}
