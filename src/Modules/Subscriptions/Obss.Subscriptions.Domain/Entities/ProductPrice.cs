using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductPrice : Entity<Guid>
{
    private ProductPrice() { }

    public ProductPrice(Guid id, PriceType priceType, string name, decimal amount, string currency,
        int? recurringPeriod = null, string? recurringPeriodUnit = null)
        : base(id)
    {
        PriceType = priceType;
        Name = name;
        Amount = amount;
        Currency = currency;
        RecurringPeriod = recurringPeriod;
        RecurringPeriodUnit = recurringPeriodUnit;
    }

    public PriceType PriceType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int? RecurringPeriod { get; private set; }
    public string? RecurringPeriodUnit { get; private set; }
}
