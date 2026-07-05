using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.ReleaseResource;

public sealed class ReleaseResourceCommandHandler : IRequestHandler<ReleaseResourceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReleaseResourceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceDto>> Handle(ReleaseResourceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdWithResourcesAsync(request.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<ServiceDto>(Error.NotFound("Service", request.ServiceId));

        var resource = service.Resources.FirstOrDefault(r => r.Id == request.ResourceId);
        if (resource is null)
            return Result.Failure<ServiceDto>(Error.NotFound("Resource", request.ResourceId));

        resource.Release();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.Adapt<ServiceDto>());
    }
}
