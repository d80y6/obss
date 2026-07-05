using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<PaginatedResult<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaginatedResult<OrderSummaryDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerAsync(
            request.CustomerId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _orderRepository.GetCountAsync(request.CustomerId, null, null, null, cancellationToken);

        var items = orders.Adapt<List<OrderSummaryDto>>();
        return Result.Success(new PaginatedResult<OrderSummaryDto>(items, totalCount));
    }
}
