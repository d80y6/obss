using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.ResumeService;

public sealed class ResumeServiceCommandHandler : IRequestHandler<ResumeServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceDto>> Handle(ResumeServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdWithResourcesAsync(request.ServiceId, cancellationToken);

        if (service is null)
            return Result.Failure<ServiceDto>(Error.NotFound("Service", request.ServiceId));

        service.Resume();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.Adapt<ServiceDto>());
    }
}
