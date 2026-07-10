using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBalance;

public sealed record GetBillingAccountBalanceQuery(
    Guid BillingAccountId,
    DateTime? AsOfDate) : IRequest<Result<AccountBalanceDto>>;
