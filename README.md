# Multi-Tenant Application (.NET)

A modern, scalable multi-tenant application built with ASP.NET Core 8, Entity Framework Core, and SQL Server. This application demonstrates best practices for implementing multi-tenancy in .NET applications.

## Features

- **Multi-Tenancy Support**: Complete tenant isolation and management
- **RESTful API**: Comprehensive API endpoints for tenant and user management
- **Entity Framework Core**: Database access with migrations support
- **Middleware-Based Tenant Resolution**: Automatic tenant identification from HTTP headers
- **CORS Support**: Cross-origin request handling
- **Scalable Architecture**: Clean separation of concerns with Domain, Infrastructure, and API layers

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

- **.NET 8**: Latest .NET runtime
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 8**: ORM for database access
- **SQL Server**: Database
- **C# 12**: Modern language features

## Getting Started

### Prerequisites

- .NET 8 SDK or higher
- SQL Server (local or remote)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository or navigate to the project folder
2. Update the connection string in `MultiTenantApp.Api/Program.cs`:
   ```csharp
   options.UseSqlServer("Server=.;Database=MultiTenantAppDb;Integrated Security=true;");
   ```

3. Install dependencies and build:
   ```bash
   cd MultiTenantApp.Api
   dotnet restore
   dotnet build
   ```

4. Create the database:
   ```bash
   dotnet ef database update
   ```

5. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001/swagger/index.html`

## API Endpoints

### Health Check
- **GET** `/api/health` - Check application status and tenant context

### Tenants Management
- **GET** `/api/tenants` - Get all tenants
- **GET** `/api/tenants/{id}` - Get tenant by ID
- **POST** `/api/tenants` - Create a new tenant
- **PUT** `/api/tenants/{id}` - Update tenant
- **DELETE** `/api/tenants/{id}` - Delete tenant

#### Create Tenant Request
```json
{
  "name": "Acme Corporation",
  "slug": "acme-corp",
  "connectionString": "Server=.;Database=AcmeDb;Integrated Security=true;"
}
```

### Tenant Users Management
- **GET** `/api/tenantusers/tenant/{tenantId}` - Get users for a tenant
- **GET** `/api/tenantusers/{id}` - Get user by ID
- **POST** `/api/tenantusers` - Create a new user
- **PUT** `/api/tenantusers/{id}` - Update user
- **DELETE** `/api/tenantusers/{id}` - Delete user

#### Create User Request
```json
{
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!"
}
```

## Multi-Tenancy Implementation

### Tenant Resolution
Tenants are resolved from the `X-Tenant-Slug` HTTP header:
```bash
curl -H "X-Tenant-Slug: acme-corp" https://localhost:5001/api/health
```

### Tenant Context
The `ITenantContext` interface provides tenant information throughout the application:
```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
}
```

### Database Isolation
- Each tenant can have separate database connections
- TenantUser records are unique per tenant (composite index on TenantId + Email)
- Tenant slug is unique across the system

## Architecture Patterns

### Repository Pattern
Generic repository pattern for data access:
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
```

### Middleware
Custom middleware for tenant resolution:
```csharp
app.UseTenantResolution();
```

### Dependency Injection
Services are registered in Program.cs:
```csharp
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddDbContext<ApplicationDbContext>(options => ...);
```

## Database Schema

### Tenants Table
- Id (PK)
- Name
- Slug (Unique)
- ConnectionString
- IsActive
- CreatedAt
- UpdatedAt

### TenantUsers Table
- Id (PK)
- TenantId (FK)
- Email
- FirstName
- LastName
- PasswordHash
- IsActive
- CreatedAt
- UpdatedAt

## Future Enhancements

- [ ] Implement JWT authentication
- [ ] Add tenant-specific database support
- [ ] Implement audit logging
- [ ] Add data encryption for sensitive fields
- [ ] Implement role-based access control (RBAC)
- [ ] Add API rate limiting
- [ ] Implement tenant usage analytics
- [ ] Add backup and recovery features
- [ ] Implement webhooks for tenant events

## Security Considerations

1. **Password Hashing**: Currently uses basic SHA256. Consider BCrypt or Argon2 in production
2. **Tenant Isolation**: Implement strict data filtering based on tenant context
3. **API Authentication**: Add JWT or OAuth2 authentication
4. **SQL Injection Prevention**: Uses parameterized queries via EF Core
5. **CORS**: Currently allows all origins - configure for production

## Development Commands

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
dotnet run

# Create a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Drop the database
dotnet ef database drop
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions, please open an issue in the repository.

## Author

Created as part of the Portfolio Projects collection.
