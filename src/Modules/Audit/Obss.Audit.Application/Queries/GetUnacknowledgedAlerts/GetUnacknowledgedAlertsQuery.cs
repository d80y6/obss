using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetUnacknowledgedAlerts;

public sealed record GetUnacknowledgedAlertsQuery : IRequest<Result<IReadOnlyList<AuditAlertDto>>>;
