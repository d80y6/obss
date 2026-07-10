using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RecordBalanceTransaction;

public sealed record RecordBalanceTransactionCommand(
    Guid BillingAccountId,
    decimal Amount,
    string TransactionType,
    string Description,
    string? ReferenceId,
    string? ReferenceType) : IRequest<Result>;
