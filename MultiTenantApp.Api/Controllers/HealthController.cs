using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ITenantContext _tenantContext;

    public HealthController(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            tenantId = _tenantContext.TenantId != Guid.Empty ? _tenantContext.TenantId : "not-set",
            tenantSlug = _tenantContext.TenantSlug ?? "not-set"
        });
    }
}
