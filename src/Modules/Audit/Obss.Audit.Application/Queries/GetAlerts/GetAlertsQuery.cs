using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAlerts;

public sealed record GetAlertsQuery(
    string? Severity,
    string? AlertType,
    bool? IsAcknowledged,
    DateTime? FromDate,
    DateTime? ToDate,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<AuditAlertDto>>>;
