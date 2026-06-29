using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.UpdateNetworkElement;

public sealed class UpdateNetworkElementCommandHandler : IRequestHandler<UpdateNetworkElementCommand, Result<NetworkElementDto>>
{
    private readonly IRepository<NetworkElement> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNetworkElementCommandHandler(IRepository<NetworkElement> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NetworkElementDto>> Handle(UpdateNetworkElementCommand request, CancellationToken cancellationToken)
    {
        var element = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (element is null)
            return Result.Failure<NetworkElementDto>(Error.NotFound("NetworkElement", request.Id));

        element.UpdateDetails(
            request.Name,
            request.Hostname,
            request.Vendor,
            request.Model,
            request.SoftwareVersion,
            request.SerialNumber,
            request.Location,
            request.ManagementIP,
            request.SNMPCommunity,
            request.IsManaged);

        await _repository.UpdateAsync(element, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(element.Adapt<NetworkElementDto>());
    }
}
