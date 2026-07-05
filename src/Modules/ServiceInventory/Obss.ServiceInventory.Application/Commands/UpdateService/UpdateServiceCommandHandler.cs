using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.UpdateService;

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdWithResourcesAsync(request.ServiceId, cancellationToken);

        if (service is null)
            return Result.Failure<ServiceDto>(Error.NotFound("Service", request.ServiceId));

        if (request.Configuration is not null)
            service.UpdateConfiguration(request.Configuration);

        if (request.Location is not null)
            service.UpdateLocation(request.Location);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.Adapt<ServiceDto>());
    }
}
