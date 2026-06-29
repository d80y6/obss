using Xunit;
using FluentAssertions;
using Obss.ApiGateway.Domain.Entities;
using Obss.ApiGateway.Domain.ValueObjects;
using Obss.ApiGateway.Infrastructure.Persistence.Repositories;

namespace Obss.ApiGateway.Tests;

public class RepositoryTests : GatewayIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveApiRoute()
    {
        using var context = CreateDbContext();
        var repository = new ApiRouteRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var route = ApiRoute.Create(
            tenantId,
            "/api/users",
            "GET",
            "UsersModule",
            "/internal/users",
            true,
            ["users:read"],
            100);

        await repository.AddAsync(route);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(route.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Path.Should().Be("/api/users");
        retrieved.Method.Should().Be("GET");
        retrieved.TargetModule.Should().Be("UsersModule");
        retrieved.TargetPath.Should().Be("/internal/users");
        retrieved.RequireAuthentication.Should().BeTrue();
        retrieved.RequiredPermissions.Should().Contain("users:read");
        retrieved.RateLimitPerMinute.Should().Be(100);
        retrieved.IsActive.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanGetActiveRoutes()
    {
        using var context = CreateDbContext();
        var repository = new ApiRouteRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeRoute = ApiRoute.Create(tenantId, "/api/active", "GET", "Module", "/target", false);
        var inactiveRoute = ApiRoute.Create(tenantId, "/api/inactive", "POST", "Module", "/target", false);
        inactiveRoute.Deactivate();

        await repository.AddAsync(activeRoute);
        await repository.AddAsync(inactiveRoute);
        await context.SaveChangesAsync();

        var activeRoutes = await repository.GetActiveRoutesAsync();

        activeRoutes.Should().Contain(r => r.Path == "/api/active");
        activeRoutes.Should().NotContain(r => r.Path == "/api/inactive");
    }

    [Fact]
    public async Task CanGetRouteByPathAndMethod()
    {
        using var context = CreateDbContext();
        var repository = new ApiRouteRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var route = ApiRoute.Create(tenantId, "/api/products", "GET", "ProductsModule", "/internal/products", false);
        await repository.AddAsync(route);
        await context.SaveChangesAsync();

        var found = await repository.GetByPathAndMethodAsync("/api/products", "GET");
        found.Should().NotBeNull();
        found!.TargetModule.Should().Be("ProductsModule");

        var notFound = await repository.GetByPathAndMethodAsync("/api/products", "POST");
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task CanAddAndRetrieveApiKey()
    {
        using var context = CreateDbContext();
        var repository = new ApiKeyRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var apiKey = ApiKey.Create(
            tenantId,
            "Production Key",
            ["api:write"],
            ["10.0.0.0/8"],
            60,
            null,
            DateTime.UtcNow.AddYears(1));

        await repository.AddAsync(apiKey);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(apiKey.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Production Key");
        retrieved.Status.Should().Be(ApiKeyStatus.Active);
        retrieved.Permissions.Should().Contain("api:write");
        retrieved.AllowedIPs.Should().Contain("10.0.0.0/8");
        retrieved.RateLimitPerMinute.Should().Be(60);
        retrieved.TenantId.Should().Be(tenantId);
        retrieved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CanGetApiKeyByHashedKey()
    {
        using var context = CreateDbContext();
        var repository = new ApiKeyRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var apiKey = ApiKey.Create(tenantId, "Key By Hash");
        await repository.AddAsync(apiKey);
        await context.SaveChangesAsync();

        var found = await repository.GetByKeyAsync(apiKey.Key);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Key By Hash");
        found.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanGetApiKeysByPartnerId()
    {
        using var context = CreateDbContext();
        var apiKeyRepository = new ApiKeyRepository(context);
        var partnerRepository = new PartnerRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var partner = Partner.Create(tenantId, "Test Partner", "Contact", "contact@test.com");
        await partnerRepository.AddAsync(partner);
        await context.SaveChangesAsync();

        var key1 = ApiKey.Create(tenantId, "Partner Key 1", partnerId: partner.Id);
        var key2 = ApiKey.Create(tenantId, "Partner Key 2", partnerId: partner.Id);
        var key3 = ApiKey.Create(tenantId, "Standalone Key");

        await apiKeyRepository.AddAsync(key1);
        await apiKeyRepository.AddAsync(key2);
        await apiKeyRepository.AddAsync(key3);
        await context.SaveChangesAsync();

        var partnerKeys = await apiKeyRepository.GetByPartnerIdAsync(partner.Id);

        partnerKeys.Should().HaveCount(2);
        partnerKeys.Should().Contain(k => k.Name == "Partner Key 1");
        partnerKeys.Should().Contain(k => k.Name == "Partner Key 2");
        partnerKeys.Should().NotContain(k => k.Name == "Standalone Key");
    }

    [Fact]
    public async Task CanRevokeApiKey()
    {
        using var context = CreateDbContext();
        var repository = new ApiKeyRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var apiKey = ApiKey.Create(tenantId, "Revocable Key");
        await repository.AddAsync(apiKey);
        await context.SaveChangesAsync();

        apiKey.Revoke();
        await repository.UpdateAsync(apiKey);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(apiKey.Id);
        retrieved!.Status.Should().Be(ApiKeyStatus.Revoked);
        retrieved.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CanAddAndRetrievePartner()
    {
        using var context = CreateDbContext();
        var repository = new PartnerRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var partner = Partner.Create(
            tenantId,
            "Gold Partner",
            "Alice Smith",
            "alice@goldpartner.com",
            ["192.168.1.0/24"],
            SlaLevel.Standard,
            25000);

        await repository.AddAsync(partner);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(partner.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Gold Partner");
        retrieved.ContactName.Should().Be("Alice Smith");
        retrieved.ContactEmail.Should().Be("alice@goldpartner.com");
        retrieved.AllowedIPs.Should().Contain("192.168.1.0/24");
        retrieved.SlaLevel.Should().Be(SlaLevel.Standard);
        retrieved.MaxRequestsPerDay.Should().Be(25000);
        retrieved.IsActive.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
        retrieved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CanGetPartnerByName()
    {
        using var context = CreateDbContext();
        var repository = new PartnerRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var partner = Partner.Create(tenantId, "Searchable Partner", "Search", "search@example.com");
        await repository.AddAsync(partner);
        await context.SaveChangesAsync();

        var found = await repository.GetByNameAsync("Searchable Partner");
        found.Should().NotBeNull();
        found!.ContactEmail.Should().Be("search@example.com");

        var notFound = await repository.GetByNameAsync("NonExistent Partner");
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task CanDeactivatePartner()
    {
        using var context = CreateDbContext();
        var repository = new PartnerRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var partner = Partner.Create(tenantId, "Temporary Partner", "Temp", "temp@example.com");
        await repository.AddAsync(partner);
        await context.SaveChangesAsync();

        partner.Deactivate();
        await repository.UpdateAsync(partner);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(partner.Id);
        retrieved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CanUpdateRouteRateLimit()
    {
        using var context = CreateDbContext();
        var repository = new ApiRouteRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var route = ApiRoute.Create(tenantId, "/api/rate-limited", "GET", "Module", "/target", false, rateLimitPerMinute: 60);
        await repository.AddAsync(route);
        await context.SaveChangesAsync();

        route.UpdateRateLimit(200);
        await repository.UpdateAsync(route);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(route.Id);
        retrieved!.RateLimitPerMinute.Should().Be(200);
    }
}
