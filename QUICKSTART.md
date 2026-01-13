# Multi-Tenant Application - Quick Start Guide

## Prerequisites

- .NET 8 SDK or higher
- SQL Server (Express, Standard, or local SQLite alternative)
- Visual Studio 2022, VS Code, or Rider

## Quick Setup (5 minutes)

### 1. Clone/Navigate to Project
```bash
cd C:\Users\USER\Desktop\PROJECTS\Portfolio\MultiTenantApp
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build Solution
```bash
dotnet build
```

### 4. Update Database Connection
Edit `MultiTenantApp.Api/Program.cs` line with `UseSqlServer`:

```csharp
// Current (local SQL Server)
options.UseSqlServer("Server=.;Database=MultiTenantAppDb;Integrated Security=true;TrustServerCertificate=true;");

// Alternative (SQL Server with Authentication)
options.UseSqlServer("Server=localhost,1433;Database=MultiTenantAppDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;");

// Alternative (SQLite for testing)
options.UseSqlite("Data Source=multitenant.db");
```

### 5. Create Database
```bash
cd MultiTenantApp.Api
dotnet ef database create
```

### 6. Run Application
```bash
dotnet run
```

**Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

## First API Calls

### Health Check
```bash
curl https://localhost:5001/api/health -k
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-13T10:30:00Z",
  "tenantId": "not-set",
  "tenantSlug": "not-set"
}
```

### Create a Tenant
```bash
curl -X POST https://localhost:5001/api/tenants -k \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Acme Corporation",
    "slug": "acme-corp",
    "connectionString": "Server=.;Database=AcmeDb;Integrated Security=true;"
  }'
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Acme Corporation",
  "slug": "acme-corp",
  "connectionString": "Server=.;Database=AcmeDb;Integrated Security=true;",
  "isActive": true,
  "createdAt": "2026-01-13T10:30:00Z",
  "updatedAt": "2026-01-13T10:30:00Z"
}
```

### Get All Tenants
```bash
curl https://localhost:5001/api/tenants -k
```

### Create a User
```bash
curl -X POST https://localhost:5001/api/tenantusers -k \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Slug: acme-corp" \
  -d '{
    "tenantId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john@acme.com",
    "firstName": "John",
    "lastName": "Doe",
    "password": "SecurePass123!"
  }'
```

### Get Users for Tenant
```bash
curl https://localhost:5001/api/tenantusers/tenant/550e8400-e29b-41d4-a716-446655440000 -k
```

## Using Swagger UI

1. Navigate to: `https://localhost:5001/swagger/index.html`
2. All endpoints are documented and can be tested interactively
3. Headers can be added in the "Try it out" section

## Project Structure

```
MultiTenantApp/
├── MultiTenantApp.Api/
│   ├── Controllers/
│   │   ├── HealthController.cs       ← Health check endpoint
│   │   ├── TenantsController.cs      ← Tenant management
│   │   └── TenantUsersController.cs  ← User management
│   └── Program.cs                    ← Configuration
├── MultiTenantApp.Domain/
│   ├── Entities/                     ← Data models
│   └── Interfaces/                   ← Contracts
├── MultiTenantApp.Infrastructure/
│   ├── Data/                         ← Database context
│   ├── Middleware/                   ← Tenant resolution
│   ├── Repositories/                 ← Data access
│   └── Services/                     ← Business logic
└── README.md, ARCHITECTURE.md
```

## Development Workflow

### Add New Entity

1. Create in `Domain/Entities/`:
```csharp
namespace MultiTenantApp.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    // ... properties
}
```

2. Add DbSet to `Infrastructure/Data/ApplicationDbContext.cs`:
```csharp
public DbSet<Product> Products { get; set; } = null!;
```

3. Configure in `OnModelCreating`:
```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired();
    entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
});
```

4. Create migration:
```bash
dotnet ef migrations add AddProduct
dotnet ef database update
```

5. Create controller in `Api/Controllers/`:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // ... endpoint methods
}
```

### Add New Feature

1. Plan in Domain layer (entities, interfaces)
2. Implement in Infrastructure layer (services, repositories)
3. Expose via API layer (controllers)
4. Test and validate

## Common Issues

### Issue: Connection String Error
**Solution:** Update the connection string in `Program.cs` to match your SQL Server configuration

### Issue: Database Already Exists
**Solution:** 
```bash
dotnet ef database drop -f
dotnet ef database create
```

### Issue: Port Already in Use
**Solution:** Change the port in `Properties/launchSettings.json`

### Issue: TrustServerCertificate Error
**Solution:** Add `TrustServerCertificate=true;` to connection string

## Testing the Multi-Tenancy

### Send Request with Tenant Slug
```bash
curl https://localhost:5001/api/health -k \
  -H "X-Tenant-Slug: acme-corp"
```

This header is captured by `TenantResolutionMiddleware` and available throughout the request.

## Database Commands

```bash
# Create database
dotnet ef database create

# Drop database
dotnet ef database drop

# Create migration
dotnet ef migrations add MigrationName

# Remove last migration (before update)
dotnet ef migrations remove

# Update database to latest migration
dotnet ef database update

# Revert to specific migration
dotnet ef database update MigrationName

# List migrations
dotnet ef migrations list
```

## Next Steps

1. ✅ Set up and run the application
2. ✅ Create sample tenants and users
3. 📋 Review [ARCHITECTURE.md](ARCHITECTURE.md) for design details
4. 🔐 Implement authentication (JWT)
5. 🔐 Add authorization (RBAC)
6. 📊 Add logging and monitoring
7. 🧪 Write unit and integration tests
8. 📈 Deploy to production

## Documentation

- **README.md** - Overview and API reference
- **ARCHITECTURE.md** - Design patterns and architecture
- **QUICKSTART.md** - This file (setup and first steps)

## Support

For issues or questions:
1. Check the Architecture guide
2. Review the code comments
3. Consult the README API documentation

## Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Multi-Tenancy Patterns](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/tenant-isolation)
