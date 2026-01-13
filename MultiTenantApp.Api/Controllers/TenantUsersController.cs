using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;

namespace MultiTenantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantUsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public TenantUsersController(ApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    [HttpGet("tenant/{tenantId}")]
    public ActionResult<IEnumerable<TenantUser>> GetUsersByTenant(Guid tenantId)
    {
        var users = _context.TenantUsers.Where(u => u.TenantId == tenantId).ToList();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantUser>> GetUser(Guid id)
    {
        var user = await _context.TenantUsers.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<TenantUser>> CreateUser(CreateTenantUserDto dto)
    {
        // Check if email already exists for this tenant
        var existingUser = _context.TenantUsers
            .FirstOrDefault(u => u.TenantId == dto.TenantId && u.Email == dto.Email);

        if (existingUser != null)
        {
            return BadRequest("Email already exists for this tenant");
        }

        var user = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TenantUsers.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateTenantUserDto dto)
    {
        var user = await _context.TenantUsers.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = dto.FirstName ?? user.FirstName;
        user.LastName = dto.LastName ?? user.LastName;
        user.IsActive = dto.IsActive ?? user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _context.TenantUsers.Update(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.TenantUsers.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.TenantUsers.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private string HashPassword(string password)
    {
        // In a real application, use proper password hashing like BCrypt
        return System.Security.Cryptography.SHA256.Create()
            .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
            .ToString();
    }
}

public class CreateTenantUserDto
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateTenantUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
}
