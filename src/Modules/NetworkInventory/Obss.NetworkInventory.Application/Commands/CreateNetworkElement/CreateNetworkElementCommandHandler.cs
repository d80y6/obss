using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Application.Commands.CreateNetworkElement;

public sealed class CreateNetworkElementCommandHandler : IRequestHandler<CreateNetworkElementCommand, Result<NetworkElementDto>>
{
    private readonly IRepository<NetworkElement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNetworkElementCommandHandler(IRepository<NetworkElement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NetworkElementDto>> Handle(CreateNetworkElementCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(request.TenantId);
        var elementType = Enum.Parse<ElementType>(request.ElementType);

        var networkElement = NetworkElement.Create(
            tenantId,
            request.Name,
            request.Hostname,
            request.IPAddress,
            elementType,
            request.Vendor,
            request.Model,
            request.SoftwareVersion,
            request.SerialNumber,
            request.Location,
            request.ManagementIP,
            request.SNMPCommunity,
            request.IsManaged);

        await _repository.AddAsync(networkElement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(networkElement.Adapt<NetworkElementDto>());
    }
}
