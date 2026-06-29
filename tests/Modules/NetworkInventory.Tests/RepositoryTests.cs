using Xunit;
using FluentAssertions;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.NetworkInventory.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Tests;

public class RepositoryTests : NetworkIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveNetworkElement()
    {
        using var context = CreateDbContext();
        var repository = new NetworkElementRepository(context);

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var networkElement = NetworkElement.Create(
            tenantId,
            "core-router-01",
            "cr01.example.com",
            "10.0.0.1",
            ElementType.Router,
            "Cisco",
            "ISR4451",
            "16.12.03",
            "SN-001",
            "DataCenter-A",
            "10.0.0.254",
            "public",
            true);

        await repository.AddAsync(networkElement);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(networkElement.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("core-router-01");
        retrieved.Hostname.Should().Be("cr01.example.com");
        retrieved.IPAddress.Should().Be("10.0.0.1");
        retrieved.ElementType.Should().Be(ElementType.Router);
        retrieved.Vendor.Should().Be("Cisco");
        retrieved.Model.Should().Be("ISR4451");
        retrieved.SoftwareVersion.Should().Be("16.12.03");
        retrieved.SerialNumber.Should().Be("SN-001");
        retrieved.Location.Should().Be("DataCenter-A");
        retrieved.Status.Should().Be(ElementStatus.Active);
        retrieved.ManagementIP.Should().Be("10.0.0.254");
        retrieved.SNMPCommunity.Should().Be("public");
        retrieved.IsManaged.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanQueryNetworkElementsByType()
    {
        using (var context = CreateDbContext())
        {
            var repo = new NetworkElementRepository(context);
            var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

            var router = NetworkElement.Create(tenantId, "router-01", "r1.example.com", "10.0.0.1", ElementType.Router, "Cisco", "ISR4451");
            var switch1 = NetworkElement.Create(tenantId, "sw-01", "sw1.example.com", "10.0.1.1", ElementType.Switch, "Juniper", "EX4300");
            var switch2 = NetworkElement.Create(tenantId, "sw-02", "sw2.example.com", "10.0.1.2", ElementType.Switch, "Juniper", "EX4300");
            var firewall = NetworkElement.Create(tenantId, "fw-01", "fw1.example.com", "10.0.2.1", ElementType.Firewall, "PaloAlto", "PA-5250");

            await repo.AddAsync(router);
            await repo.AddAsync(switch1);
            await repo.AddAsync(switch2);
            await repo.AddAsync(firewall);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new NetworkElementRepository(context);
            var switches = await repo.GetFilteredAsync("Switch", null, null, 1, 10);

            switches.Should().HaveCount(2);
            switches.Should().Contain(e => e.Name == "sw-01");
            switches.Should().Contain(e => e.Name == "sw-02");
            switches.Should().NotContain(e => e.Name == "router-01");
            switches.Should().NotContain(e => e.Name == "fw-01");
        }
    }

    [Fact]
    public async Task CanGetByHostname()
    {
        using var context = CreateDbContext();
        var repo = new NetworkElementRepository(context);
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

        var element = NetworkElement.Create(tenantId, "unique-host", "unique.example.com", "10.0.0.1", ElementType.Server, "HP", "DL380");
        await repo.AddAsync(element);
        await context.SaveChangesAsync();

        var found = await repo.GetByHostnameAsync("unique.example.com");
        found.Should().NotBeNull();
        found!.Name.Should().Be("unique-host");

        var notFound = await repo.GetByHostnameAsync("nonexistent.example.com");
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task CanFilterNetworkElementsByLocation()
    {
        using var context = CreateDbContext();
        var repo = new NetworkElementRepository(context);
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

        var dcA = NetworkElement.Create(tenantId, "dc-a-rtr", "dc-a.example.com", "10.0.0.1", ElementType.Router, "Cisco", "ISR4451", location: "DataCenter-A");
        var dcB = NetworkElement.Create(tenantId, "dc-b-rtr", "dc-b.example.com", "10.0.1.1", ElementType.Router, "Cisco", "ISR4451", location: "DataCenter-B");
        var remote = NetworkElement.Create(tenantId, "remote-rtr", "remote.example.com", "10.0.2.1", ElementType.Router, "Cisco", "ISR4451", location: "Remote-Office");

        await repo.AddAsync(dcA);
        await repo.AddAsync(dcB);
        await repo.AddAsync(remote);
        await context.SaveChangesAsync();

        var dcResults = await repo.GetFilteredAsync(null, null, "DataCenter", 1, 10);
        dcResults.Should().HaveCount(2);
        dcResults.Should().Contain(e => e.Name == "dc-a-rtr");
        dcResults.Should().Contain(e => e.Name == "dc-b-rtr");
        dcResults.Should().NotContain(e => e.Name == "remote-rtr");
    }

    [Fact]
    public async Task CanUpdateNetworkElementStatus()
    {
        using var context = CreateDbContext();
        var repo = new NetworkElementRepository(context);
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

        var element = NetworkElement.Create(tenantId, "test-element", "test.example.com", "10.0.0.1", ElementType.Switch, "Cisco", "Catalyst 9300");
        await repo.AddAsync(element);
        await context.SaveChangesAsync();

        element.SetMaintenance();
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(element.Id);
        retrieved!.Status.Should().Be(ElementStatus.Maintenance);
    }
}
