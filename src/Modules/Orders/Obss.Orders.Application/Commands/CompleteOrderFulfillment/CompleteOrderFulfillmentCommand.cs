using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CompleteOrderFulfillment;

public sealed record CompleteOrderFulfillmentCommand(
    Guid OrderId,
    bool IsSuccess,
    string? ErrorMessage) : IRequest<Result>;
