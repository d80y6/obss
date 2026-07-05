using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.RemoveTopologyLink;

public sealed class RemoveTopologyLinkCommandHandler : IRequestHandler<RemoveTopologyLinkCommand, Result>
{
    private readonly IServiceTopologyRepository _topologyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTopologyLinkCommandHandler(IServiceTopologyRepository topologyRepository, IUnitOfWork unitOfWork)
    {
        _topologyRepository = topologyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveTopologyLinkCommand request, CancellationToken cancellationToken)
    {
        var topology = await _topologyRepository.GetByIdWithLinksAsync(request.ServiceTopologyId, cancellationToken);

        if (topology is null)
            return Result.Failure(Error.NotFound("ServiceTopology", request.ServiceTopologyId));

        topology.RemoveLink(request.LinkId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
