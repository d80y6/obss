using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.ApplyTaxExemption;

public sealed record ApplyTaxExemptionCommand(
    Guid CustomerId,
    Guid? BillingAccountId,
    Guid TaxRuleId,
    string ExemptionCertificate,
    decimal ExemptionRate,
    DateTime ValidFrom,
    DateTime ValidTo,
    string ApprovedBy) : IRequest<Result<TaxExemptionDto>>;
