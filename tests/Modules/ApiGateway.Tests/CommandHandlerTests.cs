using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.ApiGateway.Application.Commands.CreateApiKey;
using Obss.ApiGateway.Application.Commands.RegisterApiRoute;
using Obss.ApiGateway.Application.Commands.RegisterPartner;
using Obss.ApiGateway.Application.Commands.RevokeApiKey;
using Obss.ApiGateway.Domain.Entities;
using Obss.ApiGateway.Domain.ValueObjects;
using Obss.ApiGateway.Infrastructure.Persistence;
using Obss.ApiGateway.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ApiGateway.Tests;

public class CommandHandlerTests : GatewayIntegrationTests
{
    [Fact]
    public async Task RegisterApiRouteCommand_ShouldCreateRouteInDatabase()
    {
        using var context = CreateDbContext();
        var routeRepository = new ApiRouteRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new RegisterApiRouteCommandHandler(routeRepository, unitOfWork, currentTenant);

        var command = new RegisterApiRouteCommand(
            "/api/users",
            "GET",
            "UsersModule",
            "/internal/users",
            true,
            ["users:read"],
            100);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Path.Should().Be("/api/users");
        result.Value.Method.Should().Be("GET");
        result.Value.TargetModule.Should().Be("UsersModule");
        result.Value.TargetPath.Should().Be("/internal/users");
        result.Value.RequireAuthentication.Should().BeTrue();
        result.Value.RequiredPermissions.Should().Contain("users:read");
        result.Value.RateLimitPerMinute.Should().Be(100);
        result.Value.IsActive.Should().BeTrue();

        var saved = await routeRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Path.Should().Be("/api/users");
    }

    [Fact]
    public async Task RegisterApiRouteCommand_ShouldFailOnDuplicatePathAndMethod()
    {
        using var context = CreateDbContext();
        var routeRepository = new ApiRouteRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new RegisterApiRouteCommandHandler(routeRepository, unitOfWork, currentTenant);

        var command = new RegisterApiRouteCommand(
            "/api/users",
            "GET",
            "UsersModule",
            "/internal/users",
            true,
            ["users:read"],
            100);

        await handler.Handle(command, CancellationToken.None);
        var duplicate = await handler.Handle(command, CancellationToken.None);

        duplicate.IsSuccess.Should().BeFalse();
        duplicate.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task CreateApiKeyCommand_ShouldCreateApiKeyInDatabase()
    {
        using var context = CreateDbContext();
        var apiKeyRepository = new ApiKeyRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateApiKeyCommandHandler(apiKeyRepository, unitOfWork, currentTenant);

        var command = new CreateApiKeyCommand(
            "Test Key",
            ["api:read"],
            ["192.168.1.1"],
            60,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Key");
        result.Value.Status.Should().Be(ApiKeyStatus.Active);
        result.Value.Permissions.Should().Contain("api:read");
        result.Value.AllowedIPs.Should().Contain("192.168.1.1");
        result.Value.RateLimitPerMinute.Should().Be(60);

        var saved = await apiKeyRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Key");
        saved.Status.Should().Be(ApiKeyStatus.Active);
    }

    [Fact]
    public async Task CreateApiKeyCommand_ShouldCreateKeyWithPartnerAssociation()
    {
        using var context = CreateDbContext();
        var apiKeyRepository = new ApiKeyRepository(context);
        var partnerRepository = new PartnerRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        var tenantId = Guid.NewGuid().ToString("N");
        currentTenant.TenantId.Returns(tenantId);

        var partner = Partner.Create(tenantId, "Test Partner", "Contact", "contact@example.com");
        await partnerRepository.AddAsync(partner);
        await context.SaveChangesAsync();

        var handler = new CreateApiKeyCommandHandler(apiKeyRepository, unitOfWork, currentTenant);

        var command = new CreateApiKeyCommand(
            "Partner Key",
            null,
            null,
            30,
            partner.Id,
            DateTime.UtcNow.AddDays(30));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PartnerId.Should().Be(partner.Id);
        result.Value.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterPartnerCommand_ShouldCreatePartnerInDatabase()
    {
        using var context = CreateDbContext();
        var partnerRepository = new PartnerRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new RegisterPartnerCommandHandler(partnerRepository, unitOfWork, currentTenant);

        var command = new RegisterPartnerCommand(
            "Acme Partner",
            "John Contact",
            "john@acme.com",
            ["10.0.0.1"],
            SlaLevel.Premium,
            50000);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Acme Partner");
        result.Value.ContactName.Should().Be("John Contact");
        result.Value.ContactEmail.Should().Be("john@acme.com");
        result.Value.AllowedIPs.Should().Contain("10.0.0.1");
        result.Value.SlaLevel.Should().Be(SlaLevel.Premium);
        result.Value.MaxRequestsPerDay.Should().Be(50000);
        result.Value.IsActive.Should().BeTrue();

        var saved = await partnerRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Acme Partner");
        saved.SlaLevel.Should().Be(SlaLevel.Premium);
    }

    [Fact]
    public async Task RegisterPartnerCommand_ShouldFailOnDuplicateName()
    {
        using var context = CreateDbContext();
        var partnerRepository = new PartnerRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new RegisterPartnerCommandHandler(partnerRepository, unitOfWork, currentTenant);

        var command = new RegisterPartnerCommand(
            "Unique Partner",
            "Contact",
            "contact@example.com",
            null,
            SlaLevel.Basic,
            10000);

        await handler.Handle(command, CancellationToken.None);
        var duplicate = await handler.Handle(command, CancellationToken.None);

        duplicate.IsSuccess.Should().BeFalse();
        duplicate.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task RevokeApiKeyCommand_ShouldRevokeKeyInDatabase()
    {
        using var context = CreateDbContext();
        var apiKeyRepository = new ApiKeyRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(tenantId);

        var apiKey = ApiKey.Create(tenantId, "Key To Revoke");
        await apiKeyRepository.AddAsync(apiKey);
        await context.SaveChangesAsync();

        var handler = new RevokeApiKeyCommandHandler(apiKeyRepository, unitOfWork);

        var result = await handler.Handle(new RevokeApiKeyCommand(apiKey.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await apiKeyRepository.GetByIdAsync(apiKey.Id);
        saved!.Status.Should().Be(ApiKeyStatus.Revoked);
        saved.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RevokeApiKeyCommand_ShouldFailForNonExistentKey()
    {
        using var context = CreateDbContext();
        var apiKeyRepository = new ApiKeyRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new RevokeApiKeyCommandHandler(apiKeyRepository, unitOfWork);

        var result = await handler.Handle(new RevokeApiKeyCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
