# OWASP API Top 10 (2023) Verification Checklist

> **Platform:** Telecom OSS/BSS Platform
> **Last Review:** 2026-06-21
> **Owner:** Security Team

---

## API1: Broken Object Level Authorization

- [ ] Every API endpoint verifies that the authenticated user owns or has permission for the requested object
- [ ] Object ID in URL path or query is validated against the authenticated principal
- [ ] No endpoint uses `FromRoute`/`FromQuery` object IDs without authorization check

```csharp
// GOOD: Ownership check
[HttpGet("{subscriberId}")]
public async Task<IActionResult> GetSubscriber(Guid subscriberId)
{
    var subscriber = await _context.Subscribers
        .FirstOrDefaultAsync(s => s.Id == subscriberId
                               && s.TenantId == _tenantProvider.GetTenantId());
    if (subscriber is null) return NotFound();
    return Ok(subscriber);
}
```

- [ ] UUIDs/GUIDs are not treated as security — they are not secrets
- [ ] Parameter tampering tests performed: changing IDs in request returns 403/404, not another user's data
- [ ] Bulk enumeration is prevented (no unbounded list endpoints without authorization)

---

## API2: Broken Authentication

- [ ] Authentication endpoint is rate limited (max 5 attempts/min per IP, 10/min per user)
- [ ] Credential stuffing protection (account lockout after 5 failed attempts, progressive delay)
- [ ] Password reset tokens are single-use and expire within 15 minutes
- [ ] API keys are rotatable and revokable
- [ ] No sensitive auth data in URLs (tokens, passwords in query strings)
- [ ] MFA enforced for admin and privileged operations

```csharp
// Example: Account lockout configuration
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
});
```

- [ ] Token revocation works correctly (logout invalidates current token)
- [ ] No "remember me" functionality without explicit consent
- [ ] Password complexity requirements enforced (min 8 chars, at least 3 of: upper, lower, digit, special)

---

## API3: Excessive Data Exposure

- [ ] API responses use DTOs/ViewModels — never serialize domain entities directly
- [ ] No `[JsonIgnore]` on entity properties as the sole data hiding mechanism (use DTOs)

```csharp
// GOOD: Explicit DTO
public record SubscriberDto(Guid Id, string Name, string ServicePlan);

// BAD: Entity leaked to response
public IActionResult GetSubscriber(Guid id)
{
    var subscriber = _context.Subscribers.Find(id);
    return Ok(subscriber);  // Exposes PasswordHash, InternalNotes, etc.
}
```

- [ ] Response fields are minimal — only what the client needs
- [ ] Collection endpoints return summaries, not full objects (unless explicitly requested)
- [ ] `$select` / field filtering (if supported) uses an allowlist, not passthrough
- [ ] Error responses do not leak internal details (stack traces, SQL errors, server paths)
- [ ] Sensitive fields (`password`, `token`, `secret`) are never included in API responses even if null/empty

---

## API4: Lack of Resources & Rate Limiting

- [ ] Rate limiting applied per API key / user / IP
- [ ] Different rate limits for different tiers (free vs premium vs internal)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ApiRateLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

- [ ] Payload size limits enforced on request body
- [ ] Compression bombs detected (max decompression ratio)
- [ ] Request timeout enforced (default 30s for API, 120s for reporting)

| Limit Type | Value |
|-----------|-------|
| Max request body | 10 MB |
| Max file upload | 50 MB |
| Max page size | 100 items |
| Max array elements in JSON | 10,000 |
| Rate limit (standard) | 100 req/min |
| Rate limit (auth) | 10 req/min |
| Request timeout | 30 seconds |

- [ ] Return `429 Too Many Requests` with `Retry-After` header when rate limited
- [ ] CPU/memory limits on serverless functions and containers

---

## API5: Broken Function Level Authorization

- [ ] Admin endpoints are not discoverable by pattern guessing (`/api/admin/` vs `/api/users/`)
- [ ] Authorization is checked in the action/endpoint, not in routing

```csharp
// GOOD
[HttpPost]
[Authorize(Policy = "Subscription.Create")]
public async Task<IActionResult> CreateSubscription(/* ... */)

// BAD - admin check only in UI
public async Task<IActionResult> DeleteUser(Guid id)
{
    // Relies on frontend hiding the button - no server-side check
}
```

- [ ] Regular users cannot access admin functionality by manipulating HTTP methods
- [ ] All roles and their permissions are documented and tested
- [ ] No endpoint uses `[Authorize]` without specifying a role, policy, or authentication scheme
- [ ] Internal/Admin APIs require separate authentication or elevated scope

---

## API6: Mass Assignment

- [ ] Input binding uses DTOs with only the allowed fields, not domain entities

```csharp
// GOOD: Explicit create DTO
public record CreateSubscriberDto(string Name, string Email, string ServicePlan);

// BAD: Direct entity binding
public async Task<IActionResult> CreateSubscriber(Subscriber subscriber)
{
    // Subscriber.IsAdmin, Subscriber.TenantId can be set by the client
}
```

- [ ] `[BindNever]` or `[Bind]` attributes are NOT trusted as sole defense — use DTOs
- [ ] No `[FromBody]` binding to Entity Framework entity classes
- [ ] PATCH/PUT operations only update fields present in the DTO
- [ ] Auto-mapper configuration ignores unmapped properties

```csharp
// AutoMapper safety
CreateMap<UpdateSubscriberDto, Subscriber>()
    .ForMember(dest => dest.TenantId, opt => opt.Ignore())
    .ForMember(dest => dest.Role, opt => opt.Ignore());
```

- [ ] TenantId, Role, IsAdmin, CreatedAt, and similar fields are never user-settable

---

## API7: Security Misconfiguration

### API-Specific Checks

- [ ] Unused HTTP methods return `405 Method Not Allowed`, not `200 OK`
- [ ] Unused endpoints are removed, not just hidden from documentation
- [ ] CORS is scoped per endpoint group where possible
- [ ] `Server` header is removed or suppressed
- [ ] `X-Powered-By` header is removed

```csharp
// Remove server headers
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// In middleware pipeline
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    await next();
});
```

- [ ] API documentation (Swagger/OpenAPI) is disabled in production
- [ ] Debug/health endpoints are restricted to internal network or authenticated access
- [ ] JSON serialization settings do not include `ReferenceHandler.Preserve` (avoids cycle exploitation)

### Transport Security

- [ ] HTTPS enforced (HTTP redirect or HSTS preload)
- [ ] API rejects requests on HTTP with 426 Upgrade Required
- [ ] gRPC endpoints use TLS

---

## API8: Injection

- [ ] All SQL is parameterized (EF Core LINQ or `FromSqlInterpolated`)
- [ ] No dynamic query construction via string concatenation
- [ ] No `Document.Eval()` or equivalent JavaScript injection vectors
- [ ] `Content-Type` is validated before parsing request body
- [ ] XML external entity (XXE) processing is disabled

```csharp
// Safe XML parsing
var settings = new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null
};
```

- [ ] No shell command execution with user input
- [ ] GraphQL endpoints have depth limiting and query cost analysis
- [ ] OData query options are restricted (no `$filter` on sensitive fields)

---

## API9: Improper Assets Management

- [ ] Deprecated API versions return `410 Gone` with a migration notice
- [ ] All API versions are documented with deprecation dates and sunset policies

```csharp
// API versioning with deprecation
[ApiVersion(1)]
[ApiVersion(2)]
[ApiVersion(3, Deprecated = true)]
[Route("api/v{version:apiVersion}/subscribers")]
```

- [ ] No shadow APIs (undocumented endpoints discovered by scanning)
- [ ] Environment-specific endpoints are not accessible from other environments
- [ ] Health checks, metrics, and debug endpoints are not exposed publicly
- [ ] API inventory is maintained and audited quarterly
- [ ] Old API gateway routes are cleaned up when services are decommissioned

### API Inventory Template

| Endpoint | Version | Status | Last Review | Deprecation Date |
|----------|---------|--------|-------------|-----------------|
| `/api/v1/subscribers` | 1.0 | Deprecated | 2026-03-01 | 2026-09-01 |
| `/api/v2/subscribers` | 2.0 | Active | 2026-06-01 | - |
| `/api/v2/billing` | 2.0 | Active | 2026-06-01 | - |

---

## API10: Unsafe Consumption of APIs

### Outbound API Calls

- [ ] All outbound API calls use HTTPS with certificate validation enabled
- [ ] `ServicePointManager.ServerCertificateValidationCallback` is never set to `true`
- [ ] Redirects are NOT automatically followed
- [ ] Timeout configured for all outbound HTTP calls (default 10s)
- [ ] Retry logic includes exponential backoff with jitter

```csharp
// Resilient HTTP client
builder.Services.AddHttpClient("PaymentGateway", client =>
{
    client.BaseAddress = new Uri("https://api.payments.example.com");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // NEVER DO THIS
});
```

- [ ] API responses are validated before processing (schema validation)
- [ ] Response size limits enforced (max 5 MB per response)
- [ ] Third-party API credentials are scoped with minimal permissions
- [ ] Third-party APIs are monitored for availability and latency
- [ ] Circuit breaker pattern implemented for critical third-party dependencies

### Third-Party Dependency Security

```csharp
builder.Services.AddHttpClient("PaymentGateway")
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)))
    .AddTransientHttpErrorPolicy(p =>
        p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt))));
```

- [ ] Third-party API keys are stored in secrets management, not config files
- [ ] Dependency on third-party APIs is documented with contact info and SLA

---

## Testing Scripts

### Functional Authorization Tests

```csharp
[Theory]
[InlineData("user", "GET", "/api/v2/subscribers/{otherUserId}")]
[InlineData("user", "DELETE", "/api/v2/subscribers")]
[InlineData("viewer", "POST", "/api/v2/billing/rates")]
public async Task UnauthorizedRoles_Get403(string role, string method, string url)
{
    // Arrange
    var client = _factory.AuthenticateAs(role);

    // Act
    var response = method switch
    {
        "GET" => await client.GetAsync(url),
        "POST" => await client.PostAsync(url, null),
        "DELETE" => await client.DeleteAsync(url),
        _ => throw new ArgumentException()
    };

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

### Rate Limiting Tests

```csharp
[Fact]
public async Task RateLimit_Exceeded_Returns429()
{
    var client = _factory.CreateClient();
    var tasks = Enumerable.Range(0, 101).Select(_ =>
        client.GetAsync("/api/v2/subscribers"));

    var results = await Task.WhenAll(tasks);
    var rateLimited = results.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

    Assert.True(rateLimited >= 1);
}
```

---

## Scan Schedule

| Scan Type | Frequency | Integration |
|-----------|-----------|-------------|
| API authorization tests | Every PR | CI pipeline |
| Rate limit validation | Weekly | Integration tests |
| Dependency audit | Weekly | Dependabot |
| API inventory validation | Quarterly | Manual |
| Penetration test | Quarterly | External |

---

## Evidence Log

| Date | API # | Check | Result | Tester |
|------|-------|-------|--------|--------|
| | | | | |
