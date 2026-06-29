using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.CreateTopology;

public sealed class CreateTopologyCommandHandler : IRequestHandler<CreateTopologyCommand, Result<ServiceTopologyDto>>
{
    private readonly IServiceTopologyRepository _topologyRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTopologyCommandHandler(
        IServiceTopologyRepository topologyRepository,
        IServiceRepository serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _topologyRepository = topologyRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceTopologyDto>> Handle(CreateTopologyCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<ServiceTopologyDto>(Error.NotFound("Service", request.ServiceId));

        if (!Enum.TryParse<TopologyType>(request.TopologyType, out var topologyType))
            return Result.Failure<ServiceTopologyDto>(Error.Validation($"Invalid topology type: '{request.TopologyType}'."));

        var existing = await _topologyRepository.GetByServiceIdWithLinksAsync(request.ServiceId, cancellationToken);
        if (existing is not null)
            return Result.Failure<ServiceTopologyDto>(Error.Conflict($"Topology already exists for service '{request.ServiceId}'."));

        var topology = ServiceTopology.Create(request.ServiceId, topologyType);

        await _topologyRepository.AddAsync(topology, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(topology.Adapt<ServiceTopologyDto>());
    }
}
