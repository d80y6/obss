using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.UpdateNasStatus;

public sealed class UpdateNasStatusCommandHandler : IRequestHandler<UpdateNasStatusCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly ICurrentTenant _currentTenant;

    public UpdateNasStatusCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork,
        IAaaAuditLogRepository logRepository,
        ICurrentTenant currentTenant)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
        _logRepository = logRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<NasDto>> Handle(UpdateNasStatusCommand request, CancellationToken cancellationToken)
    {
        var nas = await _nasRepository.GetByIdAsync(request.NasId, cancellationToken);

        if (nas is null)
            return Result.Failure<NasDto>(Error.NotFound("NetworkAccessServer", request.NasId));

        switch (request.Status.ToUpperInvariant())
        {
            case "ACTIVE":
                nas.Activate();
                break;
            case "INACTIVE":
                nas.Deactivate();
                break;
            default:
                return Result.Failure<NasDto>(Error.Validation($"Invalid status '{request.Status}'. Valid values are 'Active' or 'Inactive'."));
        }

        await _nasRepository.UpdateAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "NasStatusChanged",
            nasId: nas.Id,
            nasIpAddress: nas.NasIpAddress,
            detail: $"{{\"name\":\"{nas.Name}\",\"newStatus\":\"{request.Status}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
