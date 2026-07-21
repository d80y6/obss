using Obss.Billing.Domain.Events;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Domain.Entities;

public class BillingCycle : Entity<Guid>, ITenantEntity
{
    private BillingCycle() { }

    private BillingCycle(
        Guid id,
        string tenantId,
        Guid customerId,
        BillingPeriod billingPeriod,
        DateTime lastBillingDate,
        DateTime nextBillingDate)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        BillingPeriod = billingPeriod;
        LastBillingDate = lastBillingDate;
        NextBillingDate = nextBillingDate;
        Status = BillingCycleStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public DateTime LastBillingDate { get; private set; }
    public DateTime NextBillingDate { get; private set; }
    public BillingCycleStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public static BillingCycle Create(
        string tenantId,
        Guid customerId,
        BillingPeriod billingPeriod,
        DateTime lastBillingDate,
        DateTime nextBillingDate)
    {
        var cycle = new BillingCycle(
            Guid.NewGuid(),
            tenantId,
            customerId,
            billingPeriod,
            lastBillingDate,
            nextBillingDate);

        cycle.AddDomainEvent(new BillingCycleGeneratedDomainEvent(customerId, billingPeriod));

        return cycle;
    }

    public void AdvanceToNextCycle()
    {
        LastBillingDate = NextBillingDate;
        NextBillingDate = CalculateNextBillingDate(NextBillingDate, BillingPeriod);
    }

    public void Close()
    {
        Status = BillingCycleStatus.Closed;
    }

    private static DateTime CalculateNextBillingDate(DateTime fromDate, BillingPeriod billingPeriod)
    {
        return billingPeriod switch
        {
            BillingPeriod.Monthly => fromDate.AddMonths(1),
            BillingPeriod.Quarterly => fromDate.AddMonths(3),
            BillingPeriod.SemiAnnual => fromDate.AddMonths(6),
            BillingPeriod.Annual => fromDate.AddYears(1),
            _ => fromDate.AddMonths(1)
        };
    }
}

public enum BillingCycleStatus
{
    Active = 1,
    Closed = 2
}
