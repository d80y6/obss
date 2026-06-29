using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.ValueObjects;

public sealed class PaymentAllocationDetail : ValueObject
{
    public PaymentAllocationDetail(decimal totalAllocated, decimal remainingBalance, string currency)
    {
        TotalAllocated = totalAllocated;
        RemainingBalance = remainingBalance;
        Currency = currency;
    }

    public decimal TotalAllocated { get; }
    public decimal RemainingBalance { get; }
    public string Currency { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalAllocated;
        yield return RemainingBalance;
        yield return Currency;
    }
}
