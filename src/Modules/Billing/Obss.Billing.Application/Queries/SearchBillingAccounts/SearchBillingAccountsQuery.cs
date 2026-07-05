using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.SearchBillingAccounts;

public sealed record SearchBillingAccountsQuery(
    Guid? CustomerId,
    string? Status) : IRequest<Result<IReadOnlyList<BillingAccountDto>>>;
