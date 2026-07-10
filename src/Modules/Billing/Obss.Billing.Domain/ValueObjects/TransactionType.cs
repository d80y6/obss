namespace Obss.Billing.Domain.ValueObjects;

public enum TransactionType
{
    Charge = 1,
    Payment = 2,
    Credit = 3,
    Debit = 4,
    Adjustment = 5,
    Refund = 6
}
