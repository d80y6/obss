using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.RegisterNas;

public sealed class RegisterNasCommandHandler : IRequestHandler<RegisterNasCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAaaAuditLogRepository _logRepository;

    public RegisterNasCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        IAaaAuditLogRepository logRepository)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logRepository = logRepository;
    }

    public async Task<Result<NasDto>> Handle(RegisterNasCommand request, CancellationToken cancellationToken)
    {
        var nas = NetworkAccessServer.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Name,
            request.NasIpAddress,
            request.NasSecret,
            request.NasType,
            request.Location);

        await _nasRepository.AddAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "NasRegistered",
            nasId: nas.Id,
            nasIpAddress: nas.NasIpAddress,
            detail: $"{{\"name\":\"{nas.Name}\",\"nasType\":\"{nas.NasType}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
