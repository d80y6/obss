using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.DeleteNas;

public sealed class DeleteNasCommandHandler : IRequestHandler<DeleteNasCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly ICurrentTenant _currentTenant;

    public DeleteNasCommandHandler(
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

    public async Task<Result<NasDto>> Handle(DeleteNasCommand request, CancellationToken cancellationToken)
    {
        var nas = await _nasRepository.GetByIdAsync(request.Id, cancellationToken);

        if (nas is null)
            return Result.Failure<NasDto>(Error.NotFound("NetworkAccessServer", request.Id));

        await _nasRepository.DeleteAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "NasDeleted",
            nasId: nas.Id,
            nasIpAddress: nas.NasIpAddress,
            detail: $"{{\"name\":\"{nas.Name}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
