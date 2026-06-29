namespace Obss.Billing.Application.DTOs;

public sealed record BillingCycleDto(
    Guid Id,
    string TenantId,
    Guid CustomerId,
    string BillingPeriod,
    DateTime LastBillingDate,
    DateTime NextBillingDate,
    string Status,
    DateTime CreatedAt);
