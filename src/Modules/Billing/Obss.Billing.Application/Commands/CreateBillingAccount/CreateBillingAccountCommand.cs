using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateBillingAccount;

public sealed record CreateBillingAccountCommand(
    Guid CustomerId,
    string AccountType,
    string Name,
    decimal CreditLimit,
    string Currency) : IRequest<Result<BillingAccountDto>>;
