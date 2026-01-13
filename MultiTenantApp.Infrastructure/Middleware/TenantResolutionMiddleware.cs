using Microsoft.AspNetCore.Http;
using MultiTenantApp.Infrastructure.Services;

namespace MultiTenantApp.Infrastructure.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        var tenantSlug = context.Request.Headers["X-Tenant-Slug"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(tenantSlug))
        {
            tenantContext.TenantSlug = tenantSlug;
            // In a real application, you would resolve the tenant ID from a database
            // based on the slug
            tenantContext.TenantId = Guid.NewGuid();
        }

        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
