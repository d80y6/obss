using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.UpdateBillingAccount;

public sealed record UpdateBillingAccountCommand(
    Guid Id,
    string Name,
    decimal CreditLimit,
    string Currency,
    string? Description) : IRequest<Result<BillingAccountDto>>;
