using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderItems;

public sealed class GetServiceOrderItemsQueryHandler : IRequestHandler<GetServiceOrderItemsQuery, Result<List<ServiceOrderItemDto>>>
{
    private readonly IServiceOrderRepository _repository;

    public GetServiceOrderItemsQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ServiceOrderItemDto>>> Handle(GetServiceOrderItemsQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.ServiceOrderId, cancellationToken);
        if (order is null)
            return Result.Failure<List<ServiceOrderItemDto>>(Error.NotFound("ServiceOrder", request.ServiceOrderId));

        return Result.Success(order.Items.Adapt<List<ServiceOrderItemDto>>());
    }
}
