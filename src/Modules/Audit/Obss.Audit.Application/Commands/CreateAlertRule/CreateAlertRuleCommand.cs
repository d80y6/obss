using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.CreateAlertRule;

public sealed record CreateAlertRuleCommand(
    string Name,
    string? Description,
    string AlertType,
    string Severity,
    int Threshold,
    int WindowMinutes,
    bool IsActive = true) : IRequest<Result<AuditAlertRuleDto>>;
