using Xunit;
using FluentAssertions;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.ServiceInventory.Infrastructure.Persistence.Repositories;

namespace Obss.ServiceInventory.Tests;

public class RepositoryTests : ServiceIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveService()
    {
        using var context = CreateDbContext();
        var repository = new ServiceRepository(context);

        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var service = Service.Create(
            tenantId,
            customerId,
            subscriptionId,
            ServiceType.FTTH,
            "SVC-REP-001",
            "Downtown",
            @"{""config"": ""value""}");

        await repository.AddAsync(service);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(service.Id);

        retrieved.Should().NotBeNull();
        retrieved!.ServiceIdentifier.Should().Be("SVC-REP-001");
        retrieved.ServiceType.Should().Be(ServiceType.FTTH);
        retrieved.Status.Should().Be(ServiceStatus.Pending);
        retrieved.Location.Should().Be("Downtown");
        retrieved.Configuration.Should().Be(@"{""config"": ""value""}");
        retrieved.CustomerId.Should().Be(customerId);
        retrieved.SubscriptionId.Should().Be(subscriptionId);
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanQueryServicesByCustomer()
    {
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        using (var context = CreateDbContext())
        {
            var repo = new ServiceRepository(context);

            var service1 = Service.Create(Guid.NewGuid(), customerId1, Guid.NewGuid(), ServiceType.FTTH, "SVC-CUST-001");
            var service2 = Service.Create(Guid.NewGuid(), customerId1, Guid.NewGuid(), ServiceType.ADSL, "SVC-CUST-002");
            var service3 = Service.Create(Guid.NewGuid(), customerId2, Guid.NewGuid(), ServiceType.VoIP, "SVC-CUST-003");

            await repo.AddAsync(service1);
            await repo.AddAsync(service2);
            await repo.AddAsync(service3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new ServiceRepository(context);
            var customer1Services = await repo.GetFilteredAsync(customerId1, null, null, 1, 10);

            customer1Services.Should().HaveCount(2);
            customer1Services.Should().Contain(s => s.ServiceIdentifier == "SVC-CUST-001");
            customer1Services.Should().Contain(s => s.ServiceIdentifier == "SVC-CUST-002");
        }
    }

    [Fact]
    public async Task CanFilterServicesByStatus()
    {
        using var context = CreateDbContext();
        var repo = new ServiceRepository(context);

        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        var activeService = Service.Create(Guid.NewGuid(), customerId, subscriptionId, ServiceType.DIA, "SVC-STATUS-ACTIVE");
        activeService.Activate();
        await repo.AddAsync(activeService);
        await context.SaveChangesAsync();

        var pendingService = Service.Create(Guid.NewGuid(), customerId, subscriptionId, ServiceType.DIA, "SVC-STATUS-PENDING");
        await repo.AddAsync(pendingService);
        await context.SaveChangesAsync();

        var activeResults = await repo.GetFilteredAsync(null, null, ServiceStatus.Active, 1, 10);
        activeResults.Should().Contain(s => s.ServiceIdentifier == "SVC-STATUS-ACTIVE");
        activeResults.Should().NotContain(s => s.ServiceIdentifier == "SVC-STATUS-PENDING");
    }

    [Fact]
    public async Task CanGetServiceByIdentifier()
    {
        using var context = CreateDbContext();
        var repo = new ServiceRepository(context);

        var service = Service.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Ethernet, "UNIQUE-ID-999");
        await repo.AddAsync(service);
        await context.SaveChangesAsync();

        var found = await repo.GetByIdentifierAsync("UNIQUE-ID-999");
        found.Should().NotBeNull();
        found!.ServiceIdentifier.Should().Be("UNIQUE-ID-999");

        var notFound = await repo.GetByIdentifierAsync("NONEXISTENT");
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task CanAddAndRetrieveServiceTopology()
    {
        using var context = CreateDbContext();
        var serviceRepo = new ServiceRepository(context);
        var topologyRepo = new ServiceTopologyRepository(context);

        var customerId = Guid.NewGuid();
        var service1 = Service.Create(Guid.NewGuid(), customerId, Guid.NewGuid(), ServiceType.VPS, "SVC-TOP-001");
        var service2 = Service.Create(Guid.NewGuid(), customerId, Guid.NewGuid(), ServiceType.DedicatedServer, "SVC-TOP-002");

        await serviceRepo.AddAsync(service1);
        await serviceRepo.AddAsync(service2);
        await context.SaveChangesAsync();

        var topology = ServiceTopology.Create(service1.Id, TopologyType.Mesh);
        var link = TopologyLink.Create(topology.Id, service1.Id, service2.Id, LinkType.ConnectedTo, Direction.Bidirectional);
        topology.AddLink(link);

        await topologyRepo.AddAsync(topology);
        await context.SaveChangesAsync();

        var retrieved = await topologyRepo.GetByServiceIdWithLinksAsync(service1.Id);

        retrieved.Should().NotBeNull();
        retrieved!.TopologyType.Should().Be(TopologyType.Mesh);
        retrieved.Links.Should().HaveCount(1);
        retrieved.Links.Should().Contain(l => l.SourceServiceId == service1.Id && l.TargetServiceId == service2.Id);
    }

    [Fact]
    public async Task CanGetUpstreamAndDownstreamLinks()
    {
        using var context = CreateDbContext();
        var serviceRepo = new ServiceRepository(context);
        var topologyRepo = new ServiceTopologyRepository(context);

        var customerId = Guid.NewGuid();
        var source = Service.Create(Guid.NewGuid(), customerId, Guid.NewGuid(), ServiceType.Hosting, "SVC-LINK-SRC");
        var target = Service.Create(Guid.NewGuid(), customerId, Guid.NewGuid(), ServiceType.Domain, "SVC-LINK-TGT");

        await serviceRepo.AddAsync(source);
        await serviceRepo.AddAsync(target);
        await context.SaveChangesAsync();

        var topology = ServiceTopology.Create(source.Id, TopologyType.Linear);
        var link = TopologyLink.Create(topology.Id, source.Id, target.Id, LinkType.DependsOn, Direction.Unidirectional);
        topology.AddLink(link);

        await topologyRepo.AddAsync(topology);
        await context.SaveChangesAsync();

        var upstream = await topologyRepo.GetUpstreamLinksAsync(target.Id);
        upstream.Should().HaveCount(1);
        upstream[0].SourceServiceId.Should().Be(source.Id);

        var downstream = await topologyRepo.GetDownstreamLinksAsync(source.Id);
        downstream.Should().HaveCount(1);
        downstream[0].TargetServiceId.Should().Be(target.Id);
    }

    [Fact]
    public async Task CanAddAndRetrieveResourceDiscoveryJob()
    {
        using var context = CreateDbContext();
        var repository = new ResourceDiscoveryJobRepository(context);

        var tenantId = Guid.NewGuid();
        var job = ResourceDiscoveryJob.Create(tenantId, DiscoveryType.NetworkScan, @"{""range"": ""10.0.0.0/24""}", "admin");

        await repository.AddAsync(job);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(job.Id);

        retrieved.Should().NotBeNull();
        retrieved!.DiscoveryType.Should().Be(DiscoveryType.NetworkScan);
        retrieved.Status.Should().Be(DiscoveryStatus.Pending);
        retrieved.CreatedBy.Should().Be("admin");
        retrieved.Configuration.Should().Be(@"{""range"": ""10.0.0.0/24""}");
    }

    [Fact]
    public async Task CanGetDiscoveryJobsByTenant()
    {
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();

        using (var context = CreateDbContext())
        {
            var repo = new ResourceDiscoveryJobRepository(context);

            var job1 = ResourceDiscoveryJob.Create(tenantId1, DiscoveryType.API, null, "user1");
            var job2 = ResourceDiscoveryJob.Create(tenantId1, DiscoveryType.SNMP, null, "user1");
            var job3 = ResourceDiscoveryJob.Create(tenantId2, DiscoveryType.Manual, null, "user2");

            await repo.AddAsync(job1);
            await repo.AddAsync(job2);
            await repo.AddAsync(job3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new ResourceDiscoveryJobRepository(context);
            var tenant1Jobs = await repo.GetByTenantAsync(tenantId1);

            tenant1Jobs.Should().HaveCount(2);

            var tenant2Jobs = await repo.GetByTenantAsync(tenantId2);
            tenant2Jobs.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task CanGetUnmatchedDiscoveryJobs()
    {
        using var context = CreateDbContext();
        var repo = new ResourceDiscoveryJobRepository(context);
        var tenantId = Guid.NewGuid();

        var matchedJob = ResourceDiscoveryJob.Create(tenantId, DiscoveryType.NetworkScan, null, "admin");
        matchedJob.Complete(10, 10);
        await repo.AddAsync(matchedJob);

        var unmatchedJob = ResourceDiscoveryJob.Create(tenantId, DiscoveryType.IPRange, null, "admin");
        unmatchedJob.Complete(10, 5);
        await repo.AddAsync(unmatchedJob);

        await context.SaveChangesAsync();

        var unmatched = await repo.GetUnmatchedAsync(tenantId);

        unmatched.Should().HaveCount(1);
        unmatched.Should().Contain(j => j.Id == unmatchedJob.Id);
        unmatched.Should().NotContain(j => j.Id == matchedJob.Id);
    }
}
