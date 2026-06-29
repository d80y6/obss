using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Application.Commands.CreateOLT;

public sealed class CreateOLTCommandHandler : IRequestHandler<CreateOLTCommand, Result<OLTDetailDto>>
{
    private readonly IRepository<OLT> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOLTCommandHandler(IRepository<OLT> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OLTDetailDto>> Handle(CreateOLTCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(request.TenantId);

        var olt = OLT.Create(
            tenantId,
            request.Name,
            request.Hostname,
            request.IPAddress,
            request.Vendor,
            request.Model,
            request.SoftwareVersion,
            request.Location,
            request.MaxPONPorts,
            request.MaxONTPerPort,
            request.MaxBandwidth);

        await _repository.AddAsync(olt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(olt.Adapt<OLTDetailDto>());
    }
}
