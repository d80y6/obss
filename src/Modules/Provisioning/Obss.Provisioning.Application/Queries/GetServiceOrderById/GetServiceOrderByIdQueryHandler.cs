using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderById;

public sealed class GetServiceOrderByIdQueryHandler : IRequestHandler<GetServiceOrderByIdQuery, Result<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;

    public GetServiceOrderByIdQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ServiceOrderDto>> Handle(GetServiceOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure<ServiceOrderDto>(Error.NotFound("ServiceOrder", request.Id));

        return Result.Success(order.Adapt<ServiceOrderDto>());
    }
}
