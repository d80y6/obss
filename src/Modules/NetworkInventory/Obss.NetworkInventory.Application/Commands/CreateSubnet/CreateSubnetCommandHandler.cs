using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Application.Commands.CreateSubnet;

public sealed class CreateSubnetCommandHandler : IRequestHandler<CreateSubnetCommand, Result<SubnetDto>>
{
    private readonly IRepository<Subnet> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubnetCommandHandler(IRepository<Subnet> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubnetDto>> Handle(CreateSubnetCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(request.TenantId);

        var subnet = Subnet.Create(
            tenantId,
            request.Network,
            request.Name,
            request.Description,
            request.Gateway,
            request.VLANId,
            request.Location);

        await _repository.AddAsync(subnet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subnet.Adapt<SubnetDto>());
    }
}
