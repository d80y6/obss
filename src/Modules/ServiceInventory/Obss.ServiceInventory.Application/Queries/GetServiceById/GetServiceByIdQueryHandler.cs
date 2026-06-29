using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServiceById;

public sealed class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceByIdQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<ServiceDto>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdWithResourcesAsync(request.ServiceId, cancellationToken);

        if (service is null)
            return Result.Failure<ServiceDto>(Error.NotFound("Service", request.ServiceId));

        return Result.Success(service.Adapt<ServiceDto>());
    }
}
