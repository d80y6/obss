using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.CreateService;

public sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceDto>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ServiceType>(request.ServiceType, out var serviceType))
            return Result.Failure<ServiceDto>(Error.Validation($"Invalid service type: '{request.ServiceType}'."));

        var existing = await _serviceRepository.GetByIdentifierAsync(request.ServiceIdentifier, cancellationToken);
        if (existing is not null)
            return Result.Failure<ServiceDto>(Error.Conflict($"A service with identifier '{request.ServiceIdentifier}' already exists."));

        var service = Service.Create(
            Guid.Empty,
            request.CustomerId,
            request.SubscriptionId,
            serviceType,
            request.ServiceIdentifier,
            request.Location,
            request.Configuration);

        await _serviceRepository.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.Adapt<ServiceDto>());
    }
}
