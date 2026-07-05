using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountById;

public sealed record GetBillingAccountByIdQuery(Guid Id) : IRequest<Result<BillingAccountDto>>;
