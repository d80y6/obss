using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.FinalizeBill;

public sealed record FinalizeBillCommand(Guid BillId) : IRequest<Result>;
