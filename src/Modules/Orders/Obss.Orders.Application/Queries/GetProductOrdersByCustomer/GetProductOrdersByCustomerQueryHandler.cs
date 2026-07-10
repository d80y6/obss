using Mapster;
using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrdersByCustomer;

public sealed class GetProductOrdersByCustomerQueryHandler : IRequestHandler<GetProductOrdersByCustomerQuery, Result<PaginatedResult<ProductOrderSummaryDto>>>
{
    private readonly IProductOrderRepository _orderRepository;

    public GetProductOrdersByCustomerQueryHandler(IProductOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaginatedResult<ProductOrderSummaryDto>>> Handle(GetProductOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerAsync(
            request.CustomerId,
            request.Offset,
            request.Limit,
            cancellationToken);

        var totalCount = await _orderRepository.GetCountAsync(request.CustomerId, null, null, null, cancellationToken);

        var items = orders.Adapt<List<ProductOrderSummaryDto>>();
        return Result.Success(new PaginatedResult<ProductOrderSummaryDto>(items, totalCount));
    }
}
