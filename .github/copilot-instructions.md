# GitHub Copilot Code Review Instructions

## Project Context
This is **boardomapi**, a .NET 8 Web API for managing IoT devices and sensor data. It uses PostgreSQL with Entity Framework Core for data persistence.

## Technology Stack
- **.NET 8** with ASP.NET Core Web API
- **Entity Framework Core 8** with Npgsql for PostgreSQL
- **C# 12** with nullable reference types enabled
- **Docker** for containerization
- **Kubernetes** for deployment (see `k8s/` folder)

---

## Code Review Checklist

### 1. C# Naming Conventions
**Review for:**
- Public members use `PascalCase`
- Private fields use `_camelCase` with underscore prefix
- Local variables and parameters use `camelCase`
- Constants use `PascalCase`
- Interfaces are prefixed with `I` (e.g., `IDeviceService`)
- Async methods are suffixed with `Async`

**Flag violations like:**
```csharp
// ❌ Bad
private readonly AppDbContext db;
public async Task<Device> getdevice(string id)

// ✅ Good
private readonly AppDbContext _db;
public async Task<Device> GetDeviceAsync(string id)
```

---

### 2. Async/Await Best Practices
**Review for:**
- All database operations use async methods (`SaveChangesAsync`, `ToListAsync`, `FirstOrDefaultAsync`, etc.)
- No `.Result` or `.Wait()` calls that can cause deadlocks
- `ConfigureAwait(false)` is NOT used in ASP.NET Core (it's unnecessary)
- Async methods return `Task` or `Task<T>`, not `void` (except event handlers)
- No async void methods in controllers

**Flag violations like:**
```csharp
// ❌ Bad - blocks thread
var devices = _db.Devices.ToList();
var result = SomeAsyncMethod().Result;

// ✅ Good - non-blocking
var devices = await _db.Devices.ToListAsync();
var result = await SomeAsyncMethod();
```

---

### 3. API Controller Standards
**Review for:**
- All controllers inherit from `ControllerBase` and have `[ApiController]` attribute
- All endpoints have explicit HTTP method attributes (`[HttpGet]`, `[HttpPost]`, etc.)
- Route templates are RESTful and use lowercase with hyphens
- Action methods return `IActionResult` or `ActionResult<T>`
- Proper use of `[FromBody]`, `[FromRoute]`, `[FromQuery]` attributes

**Flag violations like:**
```csharp
// ❌ Bad - missing attributes, wrong return type
public Device GetDevice(string id)

// ✅ Good - explicit attributes, proper return type
[HttpGet("{id}")]
public async Task<ActionResult<Device>> GetDeviceAsync([FromRoute] string id)
```

---

### 4. Input Validation & Error Handling
**Review for:**
- All user inputs are validated before use
- `string.IsNullOrWhiteSpace()` is used for string validation
- `BadRequest()` returns descriptive error messages with consistent format
- No raw exceptions are exposed to clients
- Null checks before accessing properties or methods
- Use of `NotFound()` when resources don't exist

**Required error response format:**
```csharp
return BadRequest(new { error = "Descriptive error message" });
return NotFound(new { error = "Device not found", deviceId = id });
```

**Flag violations like:**
```csharp
// ❌ Bad - no validation, exposes exception
public async Task<IActionResult> CreateDevice([FromBody] DeviceRequest request)
{
    var device = new Device { Name = request.Name };
    // Missing validation!
}

// ✅ Good - validates input
public async Task<IActionResult> CreateDevice([FromBody] DeviceRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return BadRequest(new { error = "Device name is required" });
    
    // proceed...
}
```

---

### 5. HTTP Status Codes
**Review for correct usage:**
| Status Code | When to Use |
|------------|-------------|
| `200 Ok()` | Successful GET, PUT, PATCH |
| `201 Created()` | Successful POST that creates a resource |
| `204 NoContent()` | Successful DELETE or update with no response body |
| `400 BadRequest()` | Invalid input or validation failure |
| `404 NotFound()` | Resource doesn't exist |
| `409 Conflict()` | Resource already exists or state conflict |
| `500` | Never return explicitly - let middleware handle |

**Flag violations like:**
```csharp
// ❌ Bad - wrong status for creation
return Ok(newDevice);

// ✅ Good - 201 with location header
return Created($"/devices/{device.Id}", device);
```

---

### 6. Entity Framework Core Patterns
**Review for:**
- No N+1 query issues - use `.Include()` for related data
- Use `AsNoTracking()` for read-only queries
- Avoid loading entire tables - use filtering and pagination
- Use `FindAsync()` for single entity by primary key
- Use `AnyAsync()` instead of `CountAsync() > 0` for existence checks
- Transactions are used for multiple related operations

**Flag violations like:**
```csharp
// ❌ Bad - N+1 query
var devices = await _db.Devices.ToListAsync();
foreach (var device in devices)
{
    var readings = device.SensorReadings; // Lazy load per device!
}

// ✅ Good - eager loading
var devices = await _db.Devices
    .Include(d => d.SensorReadings)
    .ToListAsync();

// ❌ Bad - inefficient existence check
if (await _db.Devices.CountAsync(d => d.Id == id) > 0)

// ✅ Good - efficient existence check
if (await _db.Devices.AnyAsync(d => d.Id == id))
```

---

### 7. Model Design
**Review for:**
- Request/Response DTOs are `record` types for immutability
- Entity models have proper primary key configuration
- Navigation properties use virtual for lazy loading support
- Required properties are marked appropriately
- DateTime properties specify UTC handling

**Flag violations like:**
```csharp
// ❌ Bad - mutable class for request
public class DeviceRequest
{
    public string Name { get; set; }
}

// ✅ Good - immutable record
public record DeviceRequest(string Name, string? Description);
```

---

### 8. Security Considerations
**Review for and flag immediately:**
- ⚠️ Connection strings or secrets in code (should be in environment variables)
- ⚠️ Logging of sensitive data (passwords, tokens, PII)
- ⚠️ SQL injection vulnerabilities (raw SQL without parameterization)
- ⚠️ Missing authorization on sensitive endpoints
- ⚠️ Returning internal error details to clients
- ⚠️ Hardcoded credentials or API keys

**Always flag:**
```csharp
// 🚨 CRITICAL - never do this
var connString = "Host=localhost;Password=secret123";
_logger.LogInformation($"User password: {user.Password}");
```

---

### 9. Code Organization
**Review for:**
- One class/record per file (with related nested types allowed)
- Files are in appropriate folders matching namespace
- Using statements are organized (System first, then third-party, then project)
- No unused using statements
- Methods are reasonably sized (< 30 lines preferred)
- Complex logic is extracted into well-named private methods

---

### 10. Documentation & Comments
**Review for:**
- Public APIs have XML documentation comments
- Complex business logic has explanatory comments
- TODO/FIXME comments have associated issue references
- Comments explain "why" not "what"

**Flag:**
```csharp
// ❌ Bad - states the obvious
// Loop through devices
foreach (var device in devices)

// ✅ Good - explains why
// Filter inactive devices first to avoid sending alerts to decommissioned hardware
var activeDevices = devices.Where(d => d.IsActive);
```

---

## Pull Request Standards

### PR Title Format
- feat: Add new feature
- fix: Bug fix
- refactor: Code refactoring
- docs: Documentation changes
- test: Adding tests
- chore: Maintenance tasks

### PR Description Must Include
1. **What** - Brief description of changes
2. **Why** - Reason for the change
3. **How** - Technical approach taken
4. **Testing** - How it was tested
5. **Breaking Changes** - Any breaking changes (if applicable)

### Automatic Rejection Criteria
Reject PRs that contain:
- Committed secrets or credentials
- Disabled security features
- Removed input validation without justification
- Direct database queries bypassing EF Core without justification
- Synchronous database calls in async context

