# Multi-Tenant Application - Architecture Guide

## Overview

This document describes the architecture and design patterns used in the MultiTenantApp application.

## Architectural Layers

### 1. Domain Layer (MultiTenantApp.Domain)

Contains the core business logic and domain models.

**Components:**
- **Entities**: `Tenant`, `TenantUser` - Pure domain objects with no external dependencies
- **Interfaces**: `ITenantContext`, `IRepository` - Contracts for infrastructure implementations

**Responsibilities:**
- Define business rules
- Represent domain concepts
- Remain agnostic to infrastructure concerns

### 2. Infrastructure Layer (MultiTenantApp.Infrastructure)

Handles all technical concerns and external system interactions.

**Components:**
- **Data**: `ApplicationDbContext` - Entity Framework Core configuration
- **Repositories**: `Repository<T>` - Generic data access pattern
- **Middleware**: `TenantResolutionMiddleware` - HTTP request processing
- **Services**: `TenantContext` - Runtime tenant context storage

**Responsibilities:**
- Database access and management
- Dependency implementations
- External system integration

### 3. API Layer (MultiTenantApp.Api)

Provides HTTP endpoints and orchestrates business operations.

**Components:**
- **Controllers**: Handle HTTP requests and responses
- **DTOs**: Data transfer objects for request/response
- **Program.cs**: Application configuration and startup

**Responsibilities:**
- Route HTTP requests
- Validate input
- Call business logic
- Return responses

## Design Patterns

### Repository Pattern

The generic `IRepository<T>` provides a abstraction for data access:

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

**Benefits:**
- Decouples business logic from data access
- Enables easier testing through mocking
- Centralizes database query logic

### Middleware Pattern

`TenantResolutionMiddleware` processes requests to extract and set tenant context:

```csharp
public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
{
    var tenantSlug = context.Request.Headers["X-Tenant-Slug"].FirstOrDefault();
    if (!string.IsNullOrEmpty(tenantSlug))
    {
        tenantContext.TenantSlug = tenantSlug;
        tenantContext.TenantId = await ResolveTenantIdAsync(tenantSlug);
    }
    await _next(context);
}
```

**Benefits:**
- Centralizes cross-cutting concerns
- Executes before each request
- Available throughout the request pipeline

### Dependency Injection

Services are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddDbContext<ApplicationDbContext>(options => ...);
```

**Scope Choices:**
- **Transient**: Created each time - stateless services
- **Scoped**: Created per request - ideal for DbContext
- **Singleton**: Created once - shared across requests

## Data Flow

### Typical Request Flow

```
HTTP Request
    ↓
TenantResolutionMiddleware (Extract tenant context)
    ↓
Route to Controller
    ↓
Controller validates input (DTO)
    ↓
Call ApplicationDbContext/Repository
    ↓
Query or modify database
    ↓
Return response (DTO)
    ↓
HTTP Response
```

## Multi-Tenancy Strategy

### Header-Based Tenant Resolution

Tenants are identified via the `X-Tenant-Slug` header:

```bash
curl -H "X-Tenant-Slug: tenant-a" https://api.example.com/api/users
```

### Database Isolation Approaches

**Current Implementation: Single Database, Multiple Tenants**
- All tenant data in one database
- Tenant ID filters queries
- Cost-effective but requires strict filtering

**Alternative: Database-Per-Tenant**
- Separate database per tenant
- Complete data isolation
- Connection string stored in Tenant entity
- More expensive but maximum isolation

### Data Filtering

Controllers filter data by tenant context:
```csharp
var users = _context.TenantUsers
    .Where(u => u.TenantId == _tenantContext.TenantId)
    .ToList();
```

## Entity Relationships

```
Tenant (1) ─── (Many) TenantUser
   │
   ├─ Id: Guid
   ├─ Name: string
   ├─ Slug: string (Unique)
   ├─ ConnectionString: string
   └─ IsActive: bool
        └─ TenantId: Guid
        ├─ Email: string (Unique per Tenant)
        ├─ FirstName: string
        ├─ LastName: string
        ├─ PasswordHash: string
        └─ IsActive: bool
```

## Configuration Management

### Connection String

Stored in `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=.;Database=MultiTenantAppDb;..."));
```

**Future Enhancement:**
```csharp
// Load from configuration
var connectionString = configuration.GetConnectionString("DefaultConnection");
```

### Environment-Specific Settings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MultiTenantAppDb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Error Handling

### Current Implementation

Controllers return appropriate HTTP status codes:
- `200 OK` - Successful GET/PUT
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Invalid input
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Unhandled exception

### Future Enhancements

Implement custom exception handling middleware:
```csharp
app.UseExceptionHandler(errorApp => 
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>();
        var response = new { error = ex?.Error.Message };
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(response);
    });
});
```

## Security Architecture

### Current Security Measures

1. **Input Validation**: DTOs validate on binding
2. **SQL Injection Prevention**: EF Core parameterized queries
3. **CORS**: Configurable cross-origin requests
4. **Tenant Isolation**: Middleware enforces tenant context

### Recommended Security Enhancements

1. **Authentication**: JWT or OAuth2
2. **Authorization**: Role-based access control (RBAC)
3. **Encryption**: Encrypt sensitive fields
4. **Audit Logging**: Track all data modifications
5. **Rate Limiting**: Prevent abuse
6. **Input Sanitization**: Escape output in views

## Performance Considerations

### Database Optimization

- Use indexes on frequently queried columns (Slug, TenantId)
- Implement pagination for large datasets
- Consider caching for read-heavy operations

### Query Optimization

```csharp
// Include related entities to prevent N+1 queries
var users = _context.TenantUsers
    .Include(u => u.Tenant)
    .ToListAsync();
```

### Async/Await

All data access is asynchronous to prevent thread starvation.

## Testing Strategy

### Unit Testing

Test domain logic and repositories:
```csharp
[Test]
public async Task CreateTenant_WithValidData_SuccessfullyCreates()
{
    // Arrange
    var repository = new Repository<Tenant>(_context);
    var tenant = new Tenant { Name = "Test" };
    
    // Act
    await repository.AddAsync(tenant);
    
    // Assert
    Assert.NotNull(tenant.Id);
}
```

### Integration Testing

Test API endpoints:
```csharp
[Test]
public async Task GetTenants_ReturnsOkWithData()
{
    var client = new HttpClient();
    var response = await client.GetAsync("/api/tenants");
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## Deployment Considerations

### Database Migrations

```bash
# Create migration
dotnet ef migrations add AddNewTable

# Apply migrations
dotnet ef database update
```

### Configuration for Production

- Use environment variables for sensitive data
- Implement proper logging
- Configure HTTPS
- Set up monitoring and alerting
- Implement backup strategy

## Scaling Strategy

### Horizontal Scaling

- Stateless API servers behind load balancer
- Shared database (or multi-database with routing)
- Distributed caching (Redis)

### Vertical Scaling

- Larger database server
- Optimize queries and indexes
- Implement connection pooling

## Future Architecture Improvements

1. **CQRS Pattern**: Separate read and write operations
2. **Event Sourcing**: Store all changes as events
3. **API Gateway**: Central point for routing and policies
4. **Message Queue**: Async processing with RabbitMQ/Kafka
5. **Microservices**: Break into smaller, focused services
6. **Container Orchestration**: Docker and Kubernetes
