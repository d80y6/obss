using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.TriggerAlert;

public sealed record TriggerAlertCommand(
    string Severity,
    string AlertType,
    string Message,
    string? EntityType = null,
    string? EntityId = null) : IRequest<Result<AuditAlertDto>>;
