using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.AddInterface;

public sealed class AddInterfaceCommandHandler : IRequestHandler<AddInterfaceCommand, Result<NetworkInterfaceDto>>
{
    private readonly IRepository<NetworkElement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddInterfaceCommandHandler(IRepository<NetworkElement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NetworkInterfaceDto>> Handle(AddInterfaceCommand request, CancellationToken cancellationToken)
    {
        var element = await _repository.GetByIdAsync(request.NetworkElementId, cancellationToken);

        if (element is null)
            return Result.Failure<NetworkInterfaceDto>(Error.NotFound("NetworkElement", request.NetworkElementId));

        var interfaceType = Enum.Parse<InterfaceType>(request.InterfaceType);

        var iface = element.AddInterface(
            request.Name,
            request.Description,
            interfaceType,
            request.Speed,
            request.MacAddress,
            request.MTU,
            request.ConnectedToInterfaceId);

        await _repository.UpdateAsync(element, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(iface.Adapt<NetworkInterfaceDto>());
    }
}
