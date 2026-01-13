namespace MultiTenantApp.Domain.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
}
