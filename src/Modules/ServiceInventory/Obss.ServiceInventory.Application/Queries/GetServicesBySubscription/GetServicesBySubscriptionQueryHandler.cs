using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServicesBySubscription;

public sealed class GetServicesBySubscriptionQueryHandler : IRequestHandler<GetServicesBySubscriptionQuery, Result<IReadOnlyList<ServiceDto>>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesBySubscriptionQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<IReadOnlyList<ServiceDto>>> Handle(GetServicesBySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetBySubscriptionAsync(request.SubscriptionId, cancellationToken);
        var result = services.Adapt<List<ServiceDto>>();
        return Result.Success<IReadOnlyList<ServiceDto>>(result);
    }
}
