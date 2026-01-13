# Multi-Tenant Application

A scalable multi-tenant SaaS application built with ASP.NET Core 8 and Entity Framework Core.

## Features

- Multi-tenant support with header-based tenant resolution
- RESTful API for tenant and user management
- Entity Framework Core with SQL Server
- Generic repository pattern for data access
- Clean architecture with Domain, Infrastructure, and API layers

## Project Structure

```
MultiTenantApp/
├── MultiTenantApp.Api/              # ASP.NET Core Web API
│   ├── Controllers/                 # API Controllers
│   │   ├── HealthController.cs
│   │   ├── TenantsController.cs
│   │   └── TenantUsersController.cs
│   └── Program.cs                   # Application configuration
├── MultiTenantApp.Domain/           # Domain Models & Interfaces
│   ├── Entities/                    # Core entities
│   │   ├── Tenant.cs
│   │   └── TenantUser.cs
│   └── Interfaces/                  # Domain contracts
│       ├── ITenantContext.cs
│       └── IRepository.cs
├── MultiTenantApp.Infrastructure/   # Data Access & Services
│   ├── Data/
│   │   └── ApplicationDbContext.cs  # EF Core DbContext
│   ├── Middleware/
│   │   └── TenantResolutionMiddleware.cs
│   ├── Repositories/
│   │   └── Repository.cs            # Generic repository pattern
│   └── Services/
│       └── TenantContext.cs          # Tenant context service
└── MultiTenantApp.sln               # Solution file

```

## Technology Stack

## Tech Stack

- .NET 8
- ASP.NET Core
- Entity Framework Core
- SQL Server

## Setup

1. Update connection string in `MultiTenantApp.Api/Program.cs`
2. Run `dotnet restore` and `dotnet build`
3. Run `dotnet ef database update` to create the database
4. Run `dotnet run` to start the application

API available at `https://localhost:5001/swagger`

## API Endpoints

### Health Check
- `GET /api/health` - Application status

### Tenants
- `GET /api/tenants` - List all tenants
- `GET /api/tenants/{id}` - Get tenant
- `POST /api/tenants` - Create tenant
- `PUT /api/tenants/{id}` - Update tenant
- `DELETE /api/tenants/{id}` - Delete tenant

**Create Tenant:**
```json
{
  "name": "Company Name",
  "slug": "company-slug",
  "connectionString": "connection-string"
}
```

### Tenant Users
- `GET /api/tenantusers/tenant/{tenantId}` - List users
- `GET /api/tenantusers/{id}` - Get user
- `POST /api/tenantusers` - Create user
- `PUT /api/tenantusers/{id}` - Update user
- `DELETE /api/tenantusers/{id}` - Delete user

**Create User:**
```json
{
  "tenantId": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "password"
}
```

## Multi-Tenancy

Tenants are resolved from the `X-Tenant-Slug` HTTP header:
```bash
curl -H "X-Tenant-Slug: company-slug" https://localhost:5001/api/health
```

## License

MIT
