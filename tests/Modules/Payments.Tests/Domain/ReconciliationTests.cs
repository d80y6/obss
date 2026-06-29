using Xunit;
using FluentAssertions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Tests.Domain;

public class ReconciliationTests
{
    [Fact]
    public void Create_ShouldSetImported()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", "statement.csv", 1000m, "USD", "admin");

        reconciliation.Status.Should().Be(ReconciliationStatus.Imported);
        reconciliation.TotalImportAmount.Should().Be(1000m);
        reconciliation.TotalReconciledAmount.Should().Be(0);
        reconciliation.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_ShouldIncreaseTotalImportAmount()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", null, 0, "USD", "admin");
        var item = CreateItem(reconciliation.Id, 500m);

        reconciliation.AddItem(item);

        reconciliation.TotalImportAmount.Should().Be(500m);
        reconciliation.Items.Should().ContainSingle();
    }

    [Fact]
    public void MarkItemMatched_ShouldUpdateStatus()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", null, 0, "USD", "admin");
        var item = CreateItem(reconciliation.Id, 300m);
        reconciliation.AddItem(item);

        reconciliation.MarkItemMatched(item.Id, Guid.NewGuid(), Guid.NewGuid());

        item.Status.Should().Be(ReconciliationItemStatus.Matched);
        reconciliation.Status.Should().Be(ReconciliationStatus.Reconciled);
        reconciliation.TotalReconciledAmount.Should().Be(300m);
    }

    [Fact]
    public void MarkItemAsDiscrepancy_ShouldSetReason()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", null, 0, "USD", "admin");
        var item = CreateItem(reconciliation.Id, 300m);
        reconciliation.AddItem(item);

        reconciliation.MarkItemAsDiscrepancy(item.Id, "No matching invoice");

        item.Status.Should().Be(ReconciliationItemStatus.Discrepancy);
        item.DiscrepancyReason.Should().Be("No matching invoice");
    }

    [Fact]
    public void MarkItemMatched_NonexistentItem_ShouldThrow()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", null, 0, "USD", "admin");

        var act = () => reconciliation.MarkItemMatched(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AutoReconcile_ShouldMatchItemsByAmountAndReference()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", null, 0, "USD", "admin");
        var item1 = CreateItem(reconciliation.Id, 500m, "TXN001");
        var item2 = CreateItem(reconciliation.Id, 300m, "TXN002");
        reconciliation.AddItem(item1);
        reconciliation.AddItem(item2);

        reconciliation.AutoReconcile(item =>
            item.ExternalReference switch
            {
                "TXN001" => (Guid.NewGuid(), Guid.NewGuid()),
                _ => ((Guid?, Guid?))(null, null)
            });

        item1.Status.Should().Be(ReconciliationItemStatus.Matched);
        item2.Status.Should().Be(ReconciliationItemStatus.Unmatched);
        reconciliation.Status.Should().Be(ReconciliationStatus.PartiallyReconciled);
    }

    [Fact]
    public void AutoReconcile_AllMatched_ShouldSetReconciled()
    {
        var reconciliation = PaymentReconciliation.Create("tenant-1", "Bank", null, 0, "USD", "admin");
        var item1 = CreateItem(reconciliation.Id, 500m, "TXN001");
        var item2 = CreateItem(reconciliation.Id, 300m, "TXN002");
        reconciliation.AddItem(item1);
        reconciliation.AddItem(item2);

        reconciliation.AutoReconcile(_ => (Guid.NewGuid(), Guid.NewGuid()));

        reconciliation.Status.Should().Be(ReconciliationStatus.Reconciled);
    }

    private static ReconciliationItem CreateItem(Guid reconciliationId, decimal amount, string reference = "REF001")
    {
        return new ReconciliationItem(Guid.NewGuid(), reconciliationId, reference, amount, "USD", DateTime.UtcNow, "Test transaction");
    }
}
