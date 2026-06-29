using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Application.Commands.CreateConnectivityLink;

public sealed class CreateConnectivityLinkCommandHandler : IRequestHandler<CreateConnectivityLinkCommand, Result<ConnectivityLinkDto>>
{
    private readonly IRepository<ConnectivityLink> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConnectivityLinkCommandHandler(IRepository<ConnectivityLink> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConnectivityLinkDto>> Handle(CreateConnectivityLinkCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(request.TenantId);
        var linkType = Enum.Parse<LinkType>(request.LinkType);

        var link = ConnectivityLink.Create(
            tenantId,
            request.Name,
            request.Description,
            request.SourceElementId,
            request.SourceInterfaceId,
            request.TargetElementId,
            request.TargetInterfaceId,
            linkType,
            request.Bandwidth,
            request.Protocol,
            request.LatencyMs,
            request.MTU);

        await _repository.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(link.Adapt<ConnectivityLinkDto>());
    }
}
