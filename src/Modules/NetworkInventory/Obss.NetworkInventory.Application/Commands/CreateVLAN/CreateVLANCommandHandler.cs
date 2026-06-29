using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Application.Commands.CreateVLAN;

public sealed class CreateVLANCommandHandler : IRequestHandler<CreateVLANCommand, Result<VLANDto>>
{
    private readonly IRepository<VLAN> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVLANCommandHandler(IRepository<VLAN> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VLANDto>> Handle(CreateVLANCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(request.TenantId);

        var vlan = VLAN.Create(
            tenantId,
            request.VLANId,
            request.Name,
            request.Description,
            request.Location);

        await _repository.AddAsync(vlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(vlan.Adapt<VLANDto>());
    }
}
