using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrderById;

public sealed class GetProductOrderByIdQueryHandler : IRequestHandler<GetProductOrderByIdQuery, Result<ProductOrderDto>>
{
    private readonly IProductOrderRepository _orderRepository;

    public GetProductOrderByIdQueryHandler(IProductOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<ProductOrderDto>> Handle(GetProductOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<ProductOrderDto>(Error.NotFound("ProductOrder", request.OrderId));

        return Result.Success(order.Adapt<ProductOrderDto>());
    }
}
