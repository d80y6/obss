using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServiceResources;

public sealed class GetServiceResourcesQueryHandler : IRequestHandler<GetServiceResourcesQuery, Result<IReadOnlyList<ServiceResourceDto>>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceResourcesQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<IReadOnlyList<ServiceResourceDto>>> Handle(GetServiceResourcesQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdWithResourcesAsync(request.ServiceId, cancellationToken);

        if (service is null)
            return Result.Failure<IReadOnlyList<ServiceResourceDto>>(Error.NotFound("Service", request.ServiceId));

        IReadOnlyList<ServiceResourceDto> resources = service.Resources.Adapt<List<ServiceResourceDto>>();
        return Result.Success(resources);
    }
}
