using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.SuspendService;

public sealed class SuspendServiceCommandHandler : IRequestHandler<SuspendServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceDto>> Handle(SuspendServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdWithResourcesAsync(request.ServiceId, cancellationToken);

        if (service is null)
            return Result.Failure<ServiceDto>(Error.NotFound("Service", request.ServiceId));

        service.Suspend(request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.Adapt<ServiceDto>());
    }
}
