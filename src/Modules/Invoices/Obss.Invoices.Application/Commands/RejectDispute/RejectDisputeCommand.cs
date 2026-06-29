using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.RejectDispute;

public sealed record RejectDisputeCommand(Guid DisputeId, string Reason) : IRequest<Result>;
