using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.AllocateIP;

public sealed class AllocateIPCommandHandler : IRequestHandler<AllocateIPCommand, Result<NetworkIpAddressDto>>
{
    private readonly IRepository<NetworkElement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AllocateIPCommandHandler(IRepository<NetworkElement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NetworkIpAddressDto>> Handle(AllocateIPCommand request, CancellationToken cancellationToken)
    {
        var element = await _repository.GetByIdAsync(request.NetworkElementId, cancellationToken);

        if (element is null)
            return Result.Failure<NetworkIpAddressDto>(Error.NotFound("NetworkElement", request.NetworkElementId));

        var addressType = Enum.Parse<AddressType>(request.AddressType);

        var addr = element.AddIpAddress(
            request.NetworkInterfaceId,
            request.IPAddress,
            request.SubnetMask,
            request.Gateway,
            addressType,
            request.AssignedTo);

        await _repository.UpdateAsync(element, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(addr.Adapt<NetworkIpAddressDto>());
    }
}
