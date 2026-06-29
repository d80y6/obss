using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.AddTopologyLink;

public sealed class AddTopologyLinkCommandHandler : IRequestHandler<AddTopologyLinkCommand, Result<TopologyLinkDto>>
{
    private readonly IServiceTopologyRepository _topologyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTopologyLinkCommandHandler(
        IServiceTopologyRepository topologyRepository,
        IUnitOfWork unitOfWork)
    {
        _topologyRepository = topologyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TopologyLinkDto>> Handle(AddTopologyLinkCommand request, CancellationToken cancellationToken)
    {
        var topology = await _topologyRepository.GetByIdAsync(request.ServiceTopologyId, cancellationToken);
        if (topology is null)
            return Result.Failure<TopologyLinkDto>(Error.NotFound("ServiceTopology", request.ServiceTopologyId));

        if (!Enum.TryParse<LinkType>(request.LinkType, out var linkType))
            return Result.Failure<TopologyLinkDto>(Error.Validation($"Invalid link type: '{request.LinkType}'."));

        if (!Enum.TryParse<Direction>(request.Direction, out var direction))
            return Result.Failure<TopologyLinkDto>(Error.Validation($"Invalid direction: '{request.Direction}'."));

        var link = TopologyLink.Create(
            request.ServiceTopologyId,
            request.SourceServiceId,
            request.TargetServiceId,
            linkType,
            direction,
            request.Attributes);

        topology.AddLink(link);
        await _topologyRepository.UpdateAsync(topology, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(link.Adapt<TopologyLinkDto>());
    }
}
