using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetTaxRules;

public sealed record GetTaxRulesQuery(
    string? Category,
    string? Country) : IRequest<Result<IReadOnlyList<TaxRuleDto>>>;
