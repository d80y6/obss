using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.PatchBillingAccount;

public sealed record PatchBillingAccountCommand(
    Guid Id,
    string? Name,
    decimal? CreditLimit,
    string? Currency,
    string? Description,
    string? Status,
    string? PaymentMethodId,
    AccountHolderDto? AccountHolder) : IRequest<Result<BillingAccountDto>>;
