using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Domain.Entities;

public class TaxExemption : Entity<Guid>, ITenantEntity
{
    private TaxExemption() { }

    private TaxExemption(
        Guid id,
        string tenantId,
        Guid customerId,
        Guid? billingAccountId,
        Guid taxRuleId,
        string exemptionCertificate,
        decimal exemptionRate,
        DateTime validFrom,
        DateTime validTo,
        string approvedBy)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        BillingAccountId = billingAccountId;
        TaxRuleId = taxRuleId;
        ExemptionCertificate = exemptionCertificate;
        ExemptionRate = exemptionRate;
        ValidFrom = validFrom;
        ValidTo = validTo;
        ApprovedBy = approvedBy;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid? BillingAccountId { get; private set; }
    public Guid TaxRuleId { get; private set; }
    public string ExemptionCertificate { get; private set; } = string.Empty;
    public decimal ExemptionRate { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public string ApprovedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public static TaxExemption Create(
        string tenantId,
        Guid customerId,
        Guid? billingAccountId,
        Guid taxRuleId,
        string exemptionCertificate,
        decimal exemptionRate,
        DateTime validFrom,
        DateTime validTo,
        string approvedBy)
    {
        return new TaxExemption(
            Guid.NewGuid(),
            tenantId,
            customerId,
            billingAccountId,
            taxRuleId,
            exemptionCertificate,
            exemptionRate,
            validFrom,
            validTo,
            approvedBy);
    }

    public bool IsValid()
    {
        var now = DateTime.UtcNow;
        return now >= ValidFrom && now <= ValidTo;
    }

    public decimal GetEffectiveRate(decimal originalRate)
    {
        return originalRate * (1m - ExemptionRate);
    }
}
