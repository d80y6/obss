using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServices;

public sealed class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, Result<IReadOnlyList<ServiceDto>>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<IReadOnlyList<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        ServiceType? serviceType = null;
        if (!string.IsNullOrWhiteSpace(request.ServiceType) && Enum.TryParse<ServiceType>(request.ServiceType, out var parsedType))
            serviceType = parsedType;

        ServiceStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ServiceStatus>(request.Status, out var parsedStatus))
            status = parsedStatus;

        var services = await _serviceRepository.GetFilteredAsync(
            request.CustomerId,
            serviceType,
            status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = services.Adapt<List<ServiceDto>>();
        return Result.Success<IReadOnlyList<ServiceDto>>(result);
    }
}
