using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AssignCustomerToSegment;

public sealed record AssignCustomerToSegmentCommand(
    Guid SegmentId,
    Guid CustomerId,
    Guid AssignedBy) : IRequest<Result>;
