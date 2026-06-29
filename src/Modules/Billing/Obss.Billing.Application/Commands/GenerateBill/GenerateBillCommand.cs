using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.GenerateBill;

public sealed record GenerateBillCommand(
    Guid CustomerId,
    string CustomerName,
    string BillingPeriod,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime DueDate,
    string Currency) : IRequest<Result<BillDto>>;
