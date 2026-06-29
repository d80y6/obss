namespace Obss.Payments.Domain.ValueObjects;

public enum ReconciliationStatus
{
    Imported,
    Reconciled,
    PartiallyReconciled,
    Failed
}
