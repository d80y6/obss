using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.AutoReconcile;

public sealed record AutoReconcileCommand(Guid ReconciliationId) : IRequest<Result>;
