using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<IReadOnlyList<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerAsync(
            request.CustomerId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = orders.Adapt<List<OrderSummaryDto>>();
        return Result.Success<IReadOnlyList<OrderSummaryDto>>(result);
    }
}
