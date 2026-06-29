using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.GenerateBillingCycle;

public sealed record GenerateBillingCycleCommand(
    Guid CustomerId,
    string BillingPeriod,
    DateTime LastBillingDate,
    DateTime NextBillingDate) : IRequest<Result<BillingCycleDto>>;
