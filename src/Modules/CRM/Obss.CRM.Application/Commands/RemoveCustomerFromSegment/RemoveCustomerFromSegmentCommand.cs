using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveCustomerFromSegment;

public sealed record RemoveCustomerFromSegmentCommand(
    Guid SegmentId,
    Guid CustomerId) : IRequest<Result>;
