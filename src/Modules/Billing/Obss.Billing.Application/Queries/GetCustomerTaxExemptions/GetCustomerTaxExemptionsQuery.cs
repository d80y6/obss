using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetCustomerTaxExemptions;

public sealed record GetCustomerTaxExemptionsQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<TaxExemptionDto>>>;
