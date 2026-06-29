using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateTaxRule;

public sealed record CreateTaxRuleCommand(
    string Name,
    string Description,
    decimal TaxRate,
    string TaxType,
    string TaxCategory,
    string Country,
    string Region,
    bool IsCompound,
    int Priority,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo) : IRequest<Result<TaxRuleDto>>;
