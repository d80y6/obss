using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.UpdateSubnet;

public sealed class UpdateSubnetCommandHandler : IRequestHandler<UpdateSubnetCommand, Result<SubnetDto>>
{
    private readonly IRepository<Subnet> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSubnetCommandHandler(IRepository<Subnet> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubnetDto>> Handle(UpdateSubnetCommand request, CancellationToken cancellationToken)
    {
        var subnet = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (subnet is null)
            return Result.Failure<SubnetDto>(Error.NotFound("Subnet", request.Id));

        subnet.Update(request.Name, request.Description, request.Gateway, request.VLANId, request.Location);

        await _repository.UpdateAsync(subnet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subnet.Adapt<SubnetDto>());
    }
}
