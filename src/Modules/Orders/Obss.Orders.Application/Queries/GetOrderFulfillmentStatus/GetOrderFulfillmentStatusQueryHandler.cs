using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrderFulfillmentStatus;

public sealed class GetOrderFulfillmentStatusQueryHandler : IRequestHandler<GetOrderFulfillmentStatusQuery, Result<OrderFulfillmentDto>>
{
    private readonly IOrderFulfillmentRepository _fulfillmentRepository;

    public GetOrderFulfillmentStatusQueryHandler(IOrderFulfillmentRepository fulfillmentRepository)
    {
        _fulfillmentRepository = fulfillmentRepository;
    }

    public async Task<Result<OrderFulfillmentDto>> Handle(GetOrderFulfillmentStatusQuery request, CancellationToken cancellationToken)
    {
        var fulfillment = await _fulfillmentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (fulfillment is null)
            return Result.Failure<OrderFulfillmentDto>(Error.NotFound("OrderFulfillment", request.OrderId));

        return Result.Success(fulfillment.Adapt<OrderFulfillmentDto>());
    }
}
