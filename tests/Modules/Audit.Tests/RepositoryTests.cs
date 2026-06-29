using Xunit;
using FluentAssertions;
using Obss.Audit.Domain.Entities;
using Obss.Audit.Domain.ValueObjects;
using Obss.Audit.Infrastructure.Persistence.Repositories;

namespace Obss.Audit.Tests;

public class RepositoryTests : AuditIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveAuditEntry()
    {
        using var context = CreateDbContext();
        var repository = new AuditEntryRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var entry = AuditEntry.Create(
            tenantId,
            "Order",
            Guid.NewGuid().ToString(),
            AuditAction.Created,
            "{\"Total\": 100}",
            Guid.NewGuid().ToString(),
            "system",
            "10.0.0.1",
            "TestAgent/1.0",
            Guid.NewGuid().ToString());

        await repository.AddAsync(entry);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(entry.Id);

        retrieved.Should().NotBeNull();
        retrieved!.TenantId.Should().Be(tenantId);
        retrieved.EntityType.Should().Be("Order");
        retrieved.Action.Should().Be(AuditAction.Created);
        retrieved.Changes.Should().Be("{\"Total\": 100}");
        retrieved.IsSensitive.Should().BeFalse();
    }

    [Fact]
    public async Task CanQueryAuditEntriesByTenant()
    {
        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new AuditEntryRepository(context);

            var entry1 = AuditEntry.Create(tenantId1, "Customer", "1", AuditAction.Created);
            var entry2 = AuditEntry.Create(tenantId1, "Customer", "2", AuditAction.Updated);
            var entry3 = AuditEntry.Create(tenantId2, "Customer", "3", AuditAction.Created);

            await repo.AddAsync(entry1);
            await repo.AddAsync(entry2);
            await repo.AddAsync(entry3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new AuditEntryRepository(context);
            var tenant1Entries = await repo.GetFilteredAsync(tenantId1, null, null, null, null, null, null, 1, 10);

            tenant1Entries.Should().HaveCount(2);
            tenant1Entries.Should().Contain(e => e.EntityId == "1");
            tenant1Entries.Should().Contain(e => e.EntityId == "2");
        }
    }

    [Fact]
    public async Task CanFilterAuditEntriesByAction()
    {
        using var context = CreateDbContext();
        var repo = new AuditEntryRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var created = AuditEntry.Create(tenantId, "Invoice", "1", AuditAction.Created);
        var deleted = AuditEntry.Create(tenantId, "Invoice", "2", AuditAction.Deleted);

        await repo.AddAsync(created);
        await repo.AddAsync(deleted);
        await context.SaveChangesAsync();

        var createdResults = await repo.GetFilteredAsync(null, null, null, "Created", null, null, null, 1, 10);
        createdResults.Should().Contain(e => e.Action == AuditAction.Created);
        createdResults.Should().NotContain(e => e.Action == AuditAction.Deleted);
    }

    [Fact]
    public async Task CanGetEntityTrail()
    {
        using var context = CreateDbContext();
        var repo = new AuditEntryRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");
        var entityId = Guid.NewGuid().ToString();

        var entry1 = AuditEntry.Create(tenantId, "Contract", entityId, AuditAction.Created);
        var entry2 = AuditEntry.Create(tenantId, "Contract", entityId, AuditAction.Updated);
        var entry3 = AuditEntry.Create(tenantId, "Contract", entityId, AuditAction.Updated);

        await repo.AddAsync(entry1);
        await repo.AddAsync(entry2);
        await repo.AddAsync(entry3);
        await context.SaveChangesAsync();

        var trail = await repo.GetEntityTrailAsync(tenantId, "Contract", entityId);

        trail.Should().HaveCount(3);
        trail.Should().BeInDescendingOrder(e => e.PerformedAt);
    }

    [Fact]
    public async Task CanAddAndRetrieveAuditAlertRule()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRuleRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var rule = AuditAlertRule.Create(
            tenantId,
            "Mass Delete Detection",
            "Detect bulk deletions",
            AlertType.DataDeletion,
            AlertSeverity.Critical,
            5,
            10);

        await repository.AddAsync(rule);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(rule.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Mass Delete Detection");
        retrieved.AlertType.Should().Be(AlertType.DataDeletion);
        retrieved.Severity.Should().Be(AlertSeverity.Critical);
        retrieved.Threshold.Should().Be(5);
        retrieved.WindowMinutes.Should().Be(10);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanGetActiveAlertRules()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRuleRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeRule = AuditAlertRule.Create(tenantId, "Active Rule", null, AlertType.FailedLogin, AlertSeverity.High, 10, 5);
        var inactiveRule = AuditAlertRule.Create(tenantId, "Inactive Rule", null, AlertType.MassExport, AlertSeverity.Medium, 50, 60);
        inactiveRule.Deactivate();

        await repository.AddAsync(activeRule);
        await repository.AddAsync(inactiveRule);
        await context.SaveChangesAsync();

        var activeRules = await repository.GetActiveRulesAsync(tenantId);

        activeRules.Should().Contain(r => r.Name == "Active Rule");
        activeRules.Should().NotContain(r => r.Name == "Inactive Rule");
    }

    [Fact]
    public async Task CanAddAndRetrieveAuditAlert()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var alert = AuditAlert.Create(
            tenantId,
            AlertSeverity.High,
            AlertType.SensitiveDataAccess,
            "Sensitive data accessed by unauthorized user",
            "Customer",
            Guid.NewGuid().ToString());

        await repository.AddAsync(alert);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(alert.Id);

        retrieved.Should().NotBeNull();
        retrieved!.TenantId.Should().Be(tenantId);
        retrieved.Severity.Should().Be(AlertSeverity.High);
        retrieved.AlertType.Should().Be(AlertType.SensitiveDataAccess);
        retrieved.Message.Should().Be("Sensitive data accessed by unauthorized user");
        retrieved.IsAcknowledged.Should().BeFalse();
    }

    [Fact]
    public async Task CanGetUnacknowledgedAlerts()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var alert1 = AuditAlert.Create(tenantId, AlertSeverity.Critical, AlertType.DataDeletion, "Bulk deletion detected");
        var alert2 = AuditAlert.Create(tenantId, AlertSeverity.Low, AlertType.AnomalousActivity, "Suspicious pattern");
        alert2.Acknowledge(Guid.NewGuid().ToString());

        await repository.AddAsync(alert1);
        await repository.AddAsync(alert2);
        await context.SaveChangesAsync();

        var unacknowledged = await repository.GetUnacknowledgedAsync(tenantId);

        unacknowledged.Should().Contain(a => a.Message == "Bulk deletion detected");
        unacknowledged.Should().NotContain(a => a.Message == "Suspicious pattern");
    }

    [Fact]
    public async Task CanFilterAlertsBySeverity()
    {
        using var context = CreateDbContext();
        var repo = new AuditAlertRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var highAlert = AuditAlert.Create(tenantId, AlertSeverity.High, AlertType.FailedLogin, "High severity alert");
        var lowAlert = AuditAlert.Create(tenantId, AlertSeverity.Low, AlertType.FailedLogin, "Low severity alert");

        await repo.AddAsync(highAlert);
        await repo.AddAsync(lowAlert);
        await context.SaveChangesAsync();

        var highResults = await repo.GetFilteredAsync(null, "High", null, null, null, null, 1, 10);
        highResults.Should().Contain(a => a.Severity == AlertSeverity.High);
        highResults.Should().NotContain(a => a.Severity == AlertSeverity.Low);
    }

    [Fact]
    public async Task CanCountAlertsByTypeAndTime()
    {
        using var context = CreateDbContext();
        var repo = new AuditAlertRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var alert1 = AuditAlert.Create(tenantId, AlertSeverity.High, AlertType.FailedLogin, "Failed login 1");
        var alert2 = AuditAlert.Create(tenantId, AlertSeverity.Medium, AlertType.FailedLogin, "Failed login 2");
        var alert3 = AuditAlert.Create(tenantId, AlertSeverity.High, AlertType.SensitiveDataAccess, "Sensitive access");

        await repo.AddAsync(alert1);
        await repo.AddAsync(alert2);
        await repo.AddAsync(alert3);
        await context.SaveChangesAsync();

        var since = DateTime.UtcNow.AddMinutes(-5);
        var failedLoginCount = await repo.GetCountByTypeAndTimeAsync(tenantId, "FailedLogin", since);
        failedLoginCount.Should().Be(2);

        var sensitiveCount = await repo.GetCountByTypeAndTimeAsync(tenantId, "SensitiveDataAccess", since);
        sensitiveCount.Should().Be(1);
    }
}
