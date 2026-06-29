using Xunit;
using FluentAssertions;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.CRM.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Tests;

public class RepositoryTests : CrmIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveCustomer()
    {
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var email = Email.Create("customer@example.com");
        var customer = Customer.Create(
            tenantId,
            CustomerType.Business,
            "Acme Corp",
            "Acme Corporation",
            "TAX-12345",
            "REG-001",
            email,
            null,
            "https://acme.example.com",
            "USD");

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(customer.Id);

        retrieved.Should().NotBeNull();
        retrieved!.DisplayName.Should().Be("Acme Corporation");
        retrieved.CompanyName.Should().Be("Acme Corp");
        retrieved.Email.Value.Should().Be("customer@example.com");
        retrieved.CustomerType.Should().Be(CustomerType.Business);
        retrieved.Status.Should().Be(CustomerStatus.Lead);
        retrieved.IsActive.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanQueryCustomersByTenant()
    {
        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new CustomerRepository(context);
            var email1 = Email.Create("user1@example.com");
            var email2 = Email.Create("user2@example.com");

            var customer1 = Customer.Create(tenantId1, CustomerType.Residential, null, "User One", null, null, email1, null, null, "USD");
            var customer2 = Customer.Create(tenantId1, CustomerType.Business, "Tenant1 Inc", "Tenant1 Inc", null, null, email2, null, null, "USD");
            var customer3 = Customer.Create(tenantId2, CustomerType.Residential, null, "User Three", null, null, Email.Create("user3@example.com"), null, null, "USD");

            await repo.AddAsync(customer1);
            await repo.AddAsync(customer2);
            await repo.AddAsync(customer3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new CustomerRepository(context);
            var tenant1Customers = await repo.GetFilteredAsync(tenantId1, null, null, null, 1, 10);

            tenant1Customers.Should().HaveCount(2);
            tenant1Customers.Should().Contain(c => c.DisplayName == "User One");
            tenant1Customers.Should().Contain(c => c.DisplayName == "Tenant1 Inc");
        }
    }

    [Fact]
    public async Task CanFilterCustomersByStatus()
    {
        using var context = CreateDbContext();
        var repo = new CustomerRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeEmail = Email.Create("active@example.com");
        var activeCustomer = Customer.Create(tenantId, CustomerType.Business, "Active Co", "Active Co", null, null, activeEmail, null, null, "USD");
        activeCustomer.Activate();
        await repo.AddAsync(activeCustomer);
        await context.SaveChangesAsync();

        var leadEmail = Email.Create("lead@example.com");
        var leadCustomer = Customer.Create(tenantId, CustomerType.Residential, null, "Lead Person", null, null, leadEmail, null, null, "USD");
        await repo.AddAsync(leadCustomer);
        await context.SaveChangesAsync();

        var activeResults = await repo.GetFilteredAsync(null, "Active", null, null, 1, 10);
        activeResults.Should().Contain(c => c.DisplayName == "Active Co");
        activeResults.Should().NotContain(c => c.DisplayName == "Lead Person");
    }

    [Fact]
    public async Task CanAddAndRetrieveCustomerSegment()
    {
        using var context = CreateDbContext();
        var repository = new CustomerSegmentRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var criteria = new SegmentCriteria([]);
        var segment = CustomerSegment.Create(tenantId, "VIP", "High-value customers", criteria, 10);

        await repository.AddAsync(segment);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(segment.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("VIP");
        retrieved.Description.Should().Be("High-value customers");
        retrieved.Priority.Should().Be(10);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanGetAllActiveSegments()
    {
        using var context = CreateDbContext();
        var repository = new CustomerSegmentRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeSegment = CustomerSegment.Create(tenantId, "Active Seg", "Active", new SegmentCriteria([]), 5);
        var inactiveSegment = CustomerSegment.Create(tenantId, "Inactive Seg", "Inactive", new SegmentCriteria([]), 3);
        inactiveSegment.Deactivate();

        await repository.AddAsync(activeSegment);
        await repository.AddAsync(inactiveSegment);
        await context.SaveChangesAsync();

        var activeSegments = await repository.GetAllActiveAsync();

        activeSegments.Should().Contain(s => s.Name == "Active Seg");
        activeSegments.Should().NotContain(s => s.Name == "Inactive Seg");
    }

    [Fact]
    public async Task CanAssignCustomerToSegment()
    {
        using var context = CreateDbContext();
        var segmentRepo = new CustomerSegmentRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var criteria = new SegmentCriteria([]);
        var segment = CustomerSegment.Create(tenantId, "Test Seg", "Test", criteria, 1);

        var customerId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        segment.AddCustomer(customerId, assignedBy, false);
        await segmentRepo.AddAsync(segment);
        await context.SaveChangesAsync();

        var isInSegment = await segmentRepo.IsCustomerInSegmentAsync(segment.Id, customerId);
        isInSegment.Should().BeTrue();

        var notInSegment = await segmentRepo.IsCustomerInSegmentAsync(segment.Id, Guid.NewGuid());
        notInSegment.Should().BeFalse();
    }
}
