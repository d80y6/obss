using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CompleteServiceOrderItem;

public sealed record CompleteServiceOrderItemCommand(
    Guid ServiceOrderId,
    Guid ItemId,
    Guid? ServiceId) : IRequest<Result>;
