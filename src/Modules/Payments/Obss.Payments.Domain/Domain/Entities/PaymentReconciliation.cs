using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Entities;

public class PaymentReconciliation : AggregateRoot<Guid>
{
    private readonly List<ReconciliationItem> _items = [];

    private PaymentReconciliation() { }

    private PaymentReconciliation(
        Guid id,
        string tenantId,
        string importSource,
        string? importFileName,
        decimal totalImportAmount,
        string currency,
        string importedBy)
        : base(id)
    {
        TenantId = tenantId;
        ImportDate = DateTime.UtcNow;
        ImportSource = importSource;
        ImportFileName = importFileName;
        Status = ReconciliationStatus.Imported;
        TotalImportAmount = totalImportAmount;
        TotalReconciledAmount = 0;
        Currency = currency;
        ImportedBy = importedBy;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public DateTime ImportDate { get; private set; }
    public string ImportSource { get; private set; } = string.Empty;
    public string? ImportFileName { get; private set; }
    public ReconciliationStatus Status { get; private set; }
    public decimal TotalImportAmount { get; private set; }
    public decimal TotalReconciledAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string ImportedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<ReconciliationItem> Items => _items.AsReadOnly();

    public static PaymentReconciliation Create(
        string tenantId,
        string importSource,
        string? importFileName,
        decimal totalImportAmount,
        string currency,
        string importedBy)
    {
        return new PaymentReconciliation(
            Guid.NewGuid(),
            tenantId,
            importSource,
            importFileName,
            totalImportAmount,
            currency,
            importedBy);
    }

    public void AddItem(ReconciliationItem item)
    {
        _items.Add(item);
        TotalImportAmount += item.Amount;
    }

    public void MarkItemMatched(Guid itemId, Guid matchedInvoiceId, Guid matchedPaymentId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            throw new InvalidOperationException($"Reconciliation item '{itemId}' not found.");

        item.Match(matchedInvoiceId, matchedPaymentId);
        TotalReconciledAmount += item.Amount;

        UpdateStatus();
    }

    public void MarkItemAsDiscrepancy(Guid itemId, string reason)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            throw new InvalidOperationException($"Reconciliation item '{itemId}' not found.");

        item.MarkAsDiscrepancy(reason);
    }

    public void AutoReconcile(Func<ReconciliationItem, (Guid? invoiceId, Guid? paymentId)> matchFunc)
    {
        foreach (var item in _items.Where(i => i.Status == ReconciliationItemStatus.Unmatched))
        {
            var (invoiceId, paymentId) = matchFunc(item);
            if (invoiceId.HasValue && paymentId.HasValue)
            {
                item.Match(invoiceId.Value, paymentId.Value);
                TotalReconciledAmount += item.Amount;
            }
        }

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (_items.Count == 0)
        {
            Status = ReconciliationStatus.Failed;
            return;
        }

        var unmatchedCount = _items.Count(i => i.Status == ReconciliationItemStatus.Unmatched);

        if (unmatchedCount == 0)
            Status = ReconciliationStatus.Reconciled;
        else if (unmatchedCount < _items.Count)
            Status = ReconciliationStatus.PartiallyReconciled;
    }
}
