using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PaginatedResult<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaginatedResult<OrderSummaryDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var orders = await _orderRepository.GetFilteredAsync(
            request.CustomerId,
            status,
            request.FromDate,
            request.ToDate,
            request.OrderType,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _orderRepository.GetCountAsync(
            request.CustomerId,
            status,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var items = orders.Adapt<List<OrderSummaryDto>>();
        return Result.Success(new PaginatedResult<OrderSummaryDto>(items, totalCount));
    }
}
