using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrders;

public sealed class GetProductOrdersQueryHandler : IRequestHandler<GetProductOrdersQuery, Result<PaginatedResult<ProductOrderSummaryDto>>>
{
    private readonly IProductOrderRepository _orderRepository;

    public GetProductOrdersQueryHandler(IProductOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaginatedResult<ProductOrderSummaryDto>>> Handle(GetProductOrdersQuery request, CancellationToken cancellationToken)
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
            request.Offset,
            request.Limit,
            cancellationToken);

        var totalCount = await _orderRepository.GetCountAsync(
            request.CustomerId,
            status,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        var items = orders.Adapt<List<ProductOrderSummaryDto>>();
        return Result.Success(new PaginatedResult<ProductOrderSummaryDto>(items, totalCount));
    }
}
