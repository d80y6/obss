using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.ScheduleReport;

public sealed record ScheduleReportCommand(
    string TenantId,
    Guid ReportDefinitionId,
    string CronExpression,
    List<string> Recipients) : IRequest<Result<ScheduledReportDto>>;
