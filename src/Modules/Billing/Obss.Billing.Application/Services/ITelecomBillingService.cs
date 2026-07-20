using Obss.Billing.Domain.ValueObjects;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Services;

public sealed record RatedUsageDto(
    Guid SubscriptionId,
    string Description,
    RecordType RecordType,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxRate,
    DateTime UsageDate,
    string Reference);

public sealed record SubscriptionChargeDto(
    Guid SubscriptionId,
    string OfferName,
    decimal MonthlyFee,
    DateTime ActivationDate,
    int Quantity);

public sealed record OneTimeChargeDto(
    string Description,
    decimal Amount,
    decimal TaxRate);

public interface ITelecomBillingService
{
    Task<Result<Guid>> GenerateBillFromUsageAsync(
        Guid customerId,
        string customerName,
        string tenantId,
        BillingPeriod period,
        DateTime periodStart,
        DateTime periodEnd,
        IReadOnlyCollection<RatedUsageDto> usageRecords,
        RatingPlan plan,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> GenerateBillWithChargesAsync(
        Guid customerId,
        string customerName,
        string tenantId,
        BillingPeriod period,
        DateTime periodStart,
        DateTime periodEnd,
        IReadOnlyCollection<RatedUsageDto> usageRecords,
        IReadOnlyCollection<SubscriptionChargeDto> subscriptions,
        IReadOnlyCollection<OneTimeChargeDto> oneTimeCharges,
        IReadOnlyCollection<DiscountRule> discounts,
        RatingPlan plan,
        CancellationToken cancellationToken = default);
}
