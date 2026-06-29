namespace Obss.Billing.Application.DTOs;

public sealed record TaxExemptionDto(
    Guid Id,
    string TenantId,
    Guid CustomerId,
    Guid TaxRuleId,
    string ExemptionCertificate,
    decimal ExemptionRate,
    DateTime ValidFrom,
    DateTime ValidTo,
    string ApprovedBy,
    DateTime CreatedAt);
