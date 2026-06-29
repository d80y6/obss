namespace Obss.Billing.Domain.ValueObjects;

public enum BillStatus
{
    Draft = 1,
    Calculated = 2,
    Finalized = 3,
    Invoiced = 4,
    Closed = 5
}
