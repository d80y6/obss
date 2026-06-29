using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.ResolveDispute;

public sealed record ResolveDisputeCommand(Guid DisputeId, string Resolution, Guid ResolvedBy) : IRequest<Result>;
