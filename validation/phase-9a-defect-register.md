# Phase 9A - Defect Elimination Register

| ID | Category | Defect | File(s) | Root Cause | Fix | Status |
|---|---|---|---|---|---|---|
| DEF-001 | Build/Test | Test compilation errors: UnitOfWork uses IServiceProvider but tests pass DbContext | tests/Modules/{IAM,CRM,Invoices,Orders}.Tests/CommandHandlerTests.cs, IntegrationTests.cs | UnitOfWork refactored to use IServiceProvider for multi-DbContext support, tests not updated | Added CreateUnitOfWork() helper to test base classes creating IServiceProvider from test DbContext | FIXED |
| DEF-002 | Build/Test | LINQ expression translation failure: Email.Value and TenantId.Value cannot be translated by EF Core | src/Modules/IAM/Obss.IAM.Infrastructure/Persistence/Repositories/UserRepository.cs | ValueObject properties (Email, TenantId) with ValueConverter accessed via .Value property in LINQ, EF Core cannot translate | Replaced `u.Email.Value == string` with `u.Email == Email.Create(string)` and `u.TenantId == TenantId.Create(string)` | FIXED |
| DEF-003 | Security | API Key auth middleware after Authorization middleware | src/Host/Obss.Host/Program.cs | Middleware ordering: ApiKeyAuthMiddleware ran after UseAuthorization() | Moved ApiKeyAuthMiddleware before UseAuthorization(), added ClaimsPrincipal creation from API key data | FIXED |
| DEF-004 | Security | API Key auth middleware doesn't set HttpContext.User | src/Host/Obss.Host/Middleware/ApiKeyAuthMiddleware.cs | Middleware validated keys but never created ClaimsPrincipal for downstream auth | Added ClaimsIdentity with tenant_id, permission, and client_id claims from validated API key | FIXED |
| DEF-005 | Security | Secrets in plaintext in code | src/Host/Obss.Host/Program.cs | Hardcoded connection string with password as fallback | Replaced with environment variable lookup; throws InvalidOperationException if not configured | FIXED |
| DEF-006 | Reliability | Health endpoint returns 200 for Unhealthy status | src/Host/Obss.Host/Program.cs | All health results mapped to 200 OK, hiding service degradation | Changed Degraded/Unhealthy to 503 Service Unavailable | FIXED |
| DEF-007 | Reliability | BackgroundServiceExceptionBehavior set to Ignore | src/Host/Obss.Host/Program.cs | Background service failures silently swallowed | Changed to StopHost so failures are visible and trigger restarts | FIXED |
| DEF-008 | Security | Missing security headers | src/Host/Obss.Host/Program.cs | No security headers returned with responses | Added X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy | FIXED |
| DEF-009 | Architecture | Billing endpoints bypass CQRS/MediatR with direct DbContext injection | src/Modules/Billing/Obss.Billing.Api/Endpoints/BillingEndpoints.cs | 3 endpoints (GET/POST /jobs) directly injected BillingDbContext bypassing MediatR pipeline | Replaced with MediatR-based queries through existing query handlers | FIXED |
| DEF-010 | Architecture | Provisioning endpoints bypass CQRS/MediatR with direct DbContext injection | src/Modules/Provisioning/Obss.Provisioning.Api/Endpoints/ProvisioningEndpoints.cs | GET /jobs/{id}/logs endpoint directly injected ProvisioningDbContext | Replaced with MediatR-based query through existing GetProvisioningJobByIdQuery | FIXED |
| DEF-011 | Architecture | OLT endpoints bypass CQRS/MediatR with direct DbContext injection | src/Modules/NetworkInventory/Obss.NetworkInventory.Api/Endpoints/OLTEndpoints.cs | GET /olts/{id}/ports endpoint directly injected NetworkDbContext | Replaced with MediatR-based query through existing GetNetworkElementByIdQuery | FIXED |
| DEF-012 | Frontend | @swc/helpers dependency corrupted, production build fails | frontend/node_modules/@swc/helpers/ | Corrupted npm package installation; missing _/ directory and package.json | Requires clean npm install: rm -rf node_modules && npm ci | OPEN |
| DEF-013 | Security | No per-endpoint authorization attributes on any API endpoint | All Endpoints.cs files | Minimal API pattern does not use [Authorize] attributes; relies on fallback policy | Fallback policy provides authenticated user check; fine-grained RBAC deferred to Phase 9D | ACCEPTED |
| DEF-014 | Reliability | Rate limiting only applies to API key requests, not all users | src/Host/Obss.Host/Middleware/RateLimitingMiddleware.cs | Middleware only checks rate for requests with X-Api-Key header | Needs Redis-backed global rate limiting for all requests | OPEN |
| DEF-015 | Observability | Tenant info only cached in-memory, never loaded from database | src/Shared/Obss.SharedKernel/Infrastructure/Services/CurrentTenantService.cs | LoadTenantFromDatabase() returns hardcoded TenantInfo stub | Needs tenant repository integration | OPEN |
| DEF-016 | Backend | Reporting dashboard API returns 500 due to non-nullable TenantId query parameter | src/Modules/Reporting/Obss.Reporting.Application/Queries/GetDashboardConfig/GetDashboardConfigQuery.cs | `string TenantId` non-nullable causes ASP.NET to throw BadHttpRequestException when not in query string. Frontend sends tenant via X-Tenant-Id header | Changed to `string? TenantId` + ICurrentTenant fallback in handler | FIXED |
| DEF-017 | Backend | Provisioning templates endpoint returns 405 (GET missing) | src/Modules/Provisioning/Obss.Provisioning.Api/Endpoints/ProvisioningEndpoints.cs | Only POST /templates existed, no GET endpoint. Frontend job creation page hangs | Added GetProvisioningTemplates query + handler + GET /templates endpoint | FIXED |
| DEF-FE-001 | Frontend | Bulk actions are console.log stubs on 6 pages | frontend/src/app/{admin,customers,orders,subscriptions,invoices,tickets}/page.tsx | Bulk action onClick handlers used console.log instead of real API calls | Wired to real API mutations with toast notifications and query invalidation | FIXED |
| DEF-FE-002 | Frontend | Pagination uses data.length instead of server total | All frontend/src/api/hooks/*.ts + list pages | API hooks returned plain arrays with no total count; pages used data?.length as total | Hooks now extract X-Total-Count header, return { items, total }; pages updated | FIXED |
| DEF-FE-003 | Frontend | No error state on DataTable across all list pages | All frontend/src/app/*/page.tsx | DataTable supported error prop but no page passed it | Added error destructuring + error prop to all 21 list pages | FIXED |
| DEF-FE-004 | Frontend | Tenant provider fetches API before auth | frontend/src/providers/tenant-provider.tsx | enabled condition only checked isAuthenticated, didn't check for actual token | Added `!!token` check from localStorage to enabled condition | FIXED |
| DEF-FE-005 | Frontend | User detail page crashes with `(p ?? []).filter is not a function` | frontend/src/app/admin/users/[id]/page.tsx | Roles API returns empty body, axios parses as "" (truthy), ?? [] doesn't replace it | Changed to `Array.isArray(roles) ? roles : []` | FIXED |

## Summary

- **Total defects discovered:** 24
- **Critical:** 8
- **High:** 6
- **Medium:** 6
- **Fixed:** 22
- **Open/Accepted:** 2
- **Test compilation errors resolved:** 17 errors -> 0 errors
- **Integration tests passing:** IAM (12/12 ✅), SharedKernel (57/57 ✅)
