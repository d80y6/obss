using Xunit;
using FluentAssertions;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;
using Obss.Ticketing.Infrastructure.Persistence.Repositories;

namespace Obss.Ticketing.Tests;

public class RepositoryTests : TicketingIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveTicket()
    {
        using var context = CreateDbContext();
        var repository = new TicketRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var ticket = Ticket.Create(
            tenantId,
            "TKT-001",
            Guid.NewGuid(),
            "Acme Corp",
            "Server down",
            "Production server is unreachable",
            TicketPriority.Critical,
            TicketCategory.Technical,
            TicketSource.Portal);

        await repository.AddAsync(ticket);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(ticket.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Subject.Should().Be("Server down");
        retrieved.Description.Should().Be("Production server is unreachable");
        retrieved.Priority.Should().Be(TicketPriority.Critical);
        retrieved.Category.Should().Be(TicketCategory.Technical);
        retrieved.Source.Should().Be(TicketSource.Portal);
        retrieved.Status.Should().Be(TicketStatus.Open);
        retrieved.TenantId.Should().Be(tenantId);
        retrieved.CustomerName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task CanQueryTicketsByTenant()
    {
        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new TicketRepository(context);

            var ticket1 = Ticket.Create(tenantId1, "TKT-T1-001", Guid.NewGuid(), "User One", "Issue 1", "Desc 1", TicketPriority.Low, TicketCategory.Billing, TicketSource.Portal);
            var ticket2 = Ticket.Create(tenantId1, "TKT-T1-002", Guid.NewGuid(), "User Two", "Issue 2", "Desc 2", TicketPriority.Medium, TicketCategory.Account, TicketSource.Email);
            var ticket3 = Ticket.Create(tenantId2, "TKT-T2-001", Guid.NewGuid(), "User Three", "Issue 3", "Desc 3", TicketPriority.High, TicketCategory.Technical, TicketSource.Phone);

            await repo.AddAsync(ticket1);
            await repo.AddAsync(ticket2);
            await repo.AddAsync(ticket3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new TicketRepository(context);
            var tenant1Tickets = await repo.GetFilteredAsync(tenantId1, null, null, null, null, null, null, null, 1, 10);

            tenant1Tickets.Should().HaveCount(2);
            tenant1Tickets.Should().Contain(t => t.Subject == "Issue 1");
            tenant1Tickets.Should().Contain(t => t.Subject == "Issue 2");
        }
    }

    [Fact]
    public async Task CanFilterTicketsByStatus()
    {
        using var context = CreateDbContext();
        var repo = new TicketRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var openTicket = Ticket.Create(tenantId, "TKT-FLT-001", Guid.NewGuid(), "Open Co", "Open ticket", "Desc", TicketPriority.Low, TicketCategory.Billing, TicketSource.Portal);
        var resolvedTicket = Ticket.Create(tenantId, "TKT-FLT-002", Guid.NewGuid(), "Resolved Co", "Resolved ticket", "Desc", TicketPriority.Medium, TicketCategory.Technical, TicketSource.Email);
        resolvedTicket.Resolve("Fixed the issue");

        await repo.AddAsync(openTicket);
        await repo.AddAsync(resolvedTicket);
        await context.SaveChangesAsync();

        var openResults = await repo.GetFilteredAsync(null, "Open", null, null, null, null, null, null, 1, 10);
        openResults.Should().Contain(t => t.Subject == "Open ticket");
        openResults.Should().NotContain(t => t.Subject == "Resolved ticket");

        var resolvedResults = await repo.GetFilteredAsync(null, "Resolved", null, null, null, null, null, null, 1, 10);
        resolvedResults.Should().Contain(t => t.Subject == "Resolved ticket");
        resolvedResults.Should().NotContain(t => t.Subject == "Open ticket");
    }

    [Fact]
    public async Task CanGetNextTicketNumber()
    {
        using var context = CreateDbContext();
        var repo = new TicketRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var first = await repo.GetNextTicketNumberAsync();
        first.Should().NotBeNullOrEmpty();
        first.Should().StartWith("TKT-");

        var ticket = Ticket.Create(tenantId, first, Guid.NewGuid(), "Test", "Test", "Test", TicketPriority.Low, TicketCategory.Other, TicketSource.API);
        await repo.AddAsync(ticket);
        await context.SaveChangesAsync();

        var second = await repo.GetNextTicketNumberAsync();
        second.Should().NotBeNullOrEmpty();
        second.Should().NotBe(first);
    }

    [Fact]
    public async Task CanAddAndRetrieveSlaDefinition()
    {
        using var context = CreateDbContext();
        var repository = new SlaDefinitionRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var sla = SlaDefinition.Create(
            tenantId,
            "Critical SLA",
            "SLA for critical priority tickets",
            TicketPriority.Critical,
            responseTimeHours: 1,
            resolutionTimeHours: 4,
            escalationTimeHours: 2);

        await repository.AddAsync(sla);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(sla.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Critical SLA");
        retrieved.Description.Should().Be("SLA for critical priority tickets");
        retrieved.Priority.Should().Be(TicketPriority.Critical);
        retrieved.ResponseTimeHours.Should().Be(1);
        retrieved.ResolutionTimeHours.Should().Be(4);
        retrieved.EscalationTimeHours.Should().Be(2);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanGetActiveSlaDefinitionsByTenant()
    {
        using var context = CreateDbContext();
        var repository = new SlaDefinitionRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeSla = SlaDefinition.Create(tenantId, "Active SLA", "Active", TicketPriority.Medium, 2, 8, 4);
        var inactiveSla = SlaDefinition.Create(tenantId, "Inactive SLA", "Inactive", TicketPriority.Low, 4, 24, 8);
        inactiveSla.Deactivate();

        await repository.AddAsync(activeSla);
        await repository.AddAsync(inactiveSla);
        await context.SaveChangesAsync();

        var activeSlas = await repository.GetActiveByTenantAsync(tenantId);

        activeSlas.Should().Contain(s => s.Name == "Active SLA");
        activeSlas.Should().NotContain(s => s.Name == "Inactive SLA");
    }

    [Fact]
    public async Task CanQueryTicketsByPriority()
    {
        using var context = CreateDbContext();
        var repo = new TicketRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var highTicket = Ticket.Create(tenantId, "TKT-PRI-001", Guid.NewGuid(), "High Co", "High priority", "Urgent", TicketPriority.High, TicketCategory.Technical, TicketSource.Portal);
        var lowTicket = Ticket.Create(tenantId, "TKT-PRI-002", Guid.NewGuid(), "Low Co", "Low priority", "Not urgent", TicketPriority.Low, TicketCategory.Billing, TicketSource.Email);

        await repo.AddAsync(highTicket);
        await repo.AddAsync(lowTicket);
        await context.SaveChangesAsync();

        var highResults = await repo.GetFilteredAsync(null, null, "High", null, null, null, null, null, 1, 10);
        highResults.Should().Contain(t => t.Subject == "High priority");
        highResults.Should().NotContain(t => t.Subject == "Low priority");
    }

    [Fact]
    public async Task CanGetOpenTickets()
    {
        using var context = CreateDbContext();
        var repo = new TicketRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var openTicket = Ticket.Create(tenantId, "TKT-OPN-001", Guid.NewGuid(), "Open Co", "Open issue", "Desc", TicketPriority.Medium, TicketCategory.ServiceRequest, TicketSource.Portal);
        var closedTicket = Ticket.Create(tenantId, "TKT-OPN-002", Guid.NewGuid(), "Closed Co", "Closed issue", "Desc", TicketPriority.Low, TicketCategory.Other, TicketSource.Portal);
        closedTicket.Resolve("Done");
        closedTicket.Close();

        await repo.AddAsync(openTicket);
        await repo.AddAsync(closedTicket);
        await context.SaveChangesAsync();

        var openTickets = await repo.GetOpenTicketsAsync(tenantId);
        openTickets.Should().Contain(t => t.Subject == "Open issue");
        openTickets.Should().NotContain(t => t.Subject == "Closed issue");
    }
}
