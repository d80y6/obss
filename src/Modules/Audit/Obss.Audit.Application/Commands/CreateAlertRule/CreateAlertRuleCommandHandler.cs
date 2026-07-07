using Mapster;
using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.CreateAlertRule;

public sealed class CreateAlertRuleCommandHandler : IRequestHandler<CreateAlertRuleCommand, Result<AuditAlertRuleDto>>
{
    private readonly IRepository<AuditAlertRule> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateAlertRuleCommandHandler(
        IRepository<AuditAlertRule> repository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<AuditAlertRuleDto>> Handle(CreateAlertRuleCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AlertType>(request.AlertType, true, out var alertType))
        {
            return Result.Failure<AuditAlertRuleDto>(Error.Validation($"Invalid alert type: '{request.AlertType}'."));
        }

        if (!Enum.TryParse<AlertSeverity>(request.Severity, true, out var severity))
        {
            return Result.Failure<AuditAlertRuleDto>(Error.Validation($"Invalid alert severity: '{request.Severity}'."));
        }

        var tenantId = _currentTenant.TenantId ?? string.Empty;

        var rule = AuditAlertRule.Create(
            tenantId,
            request.Name,
            request.Description,
            alertType,
            severity,
            request.Threshold,
            request.WindowMinutes,
            request.IsActive);

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.Adapt<AuditAlertRuleDto>());
    }
}
