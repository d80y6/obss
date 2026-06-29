using Xunit;
using FluentAssertions;
using Obss.IAM.Domain.Entities;
using Obss.IAM.Domain.Events;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Domain;

public class TenantTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        tenant.Should().NotBeNull();
        tenant.Name.Should().Be("Test Tenant");
        tenant.Slug.Should().Be("test-tenant");
        tenant.IsActive.Should().BeTrue();
        tenant.ConnectionString.Should().BeNull();
        tenant.Settings.Should().BeNull();
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithConnectionStringAndSettings_ShouldSetOptionalProperties()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant", "Host=localhost;Database=test", "{\"key\":\"value\"}");

        tenant.ConnectionString.Should().Be("Host=localhost;Database=test");
        tenant.Settings.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public void Create_ShouldRaiseTenantProvisionedDomainEvent()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        tenant.DomainEvents.Should().ContainSingle(e => e is TenantProvisionedDomainEvent);
        var domainEvent = tenant.DomainEvents.First() as TenantProvisionedDomainEvent;
        domainEvent!.Name.Should().Be("Test Tenant");
        domainEvent.Slug.Should().Be("test-tenant");
        domainEvent.TenantId.Value.Should().Be(tenant.Id.ToString("N"));
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");
        tenant.Deactivate();

        tenant.Activate();

        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotChange()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        tenant.Activate();

        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        tenant.Deactivate();

        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotChange()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");
        tenant.Deactivate();

        tenant.Deactivate();

        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateSettings_ShouldUpdate()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        tenant.UpdateSettings("{\"new\":\"settings\"}");

        tenant.Settings.Should().Be("{\"new\":\"settings\"}");
    }

    [Fact]
    public void UpdateConnectionString_ShouldUpdate()
    {
        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        tenant.UpdateConnectionString("Host=localhost;Database=newdb");

        tenant.ConnectionString.Should().Be("Host=localhost;Database=newdb");
    }
}
