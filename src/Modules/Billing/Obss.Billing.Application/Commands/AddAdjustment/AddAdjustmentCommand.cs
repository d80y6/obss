using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.AddAdjustment;

public sealed record AddAdjustmentCommand(
    Guid BillId,
    string Description,
    decimal Amount,
    string Currency) : IRequest<Result>;
