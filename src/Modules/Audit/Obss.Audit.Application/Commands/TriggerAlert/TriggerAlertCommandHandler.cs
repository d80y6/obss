using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.TriggerAlert;

public sealed class TriggerAlertCommandHandler : IRequestHandler<TriggerAlertCommand, Result<AuditAlertDto>>
{
    private readonly IAuditAlertRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public TriggerAlertCommandHandler(
        IAuditAlertRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<AuditAlertDto>> Handle(TriggerAlertCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AlertSeverity>(request.Severity, true, out var severity))
        {
            return Result.Failure<AuditAlertDto>(Error.Validation($"Invalid alert severity: '{request.Severity}'."));
        }

        if (!Enum.TryParse<AlertType>(request.AlertType, true, out var alertType))
        {
            return Result.Failure<AuditAlertDto>(Error.Validation($"Invalid alert type: '{request.AlertType}'."));
        }

        var tenantId = _currentTenant.TenantId ?? string.Empty;

        var alert = AuditAlert.Create(
            tenantId,
            severity,
            alertType,
            request.Message,
            request.EntityType,
            request.EntityId);

        await _repository.AddAsync(alert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(alert.Adapt<AuditAlertDto>());
    }
}
