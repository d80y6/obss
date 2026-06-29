using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetScheduledReports;

public sealed record GetScheduledReportsQuery(
    string? TenantId) : IRequest<Result<IReadOnlyList<ScheduledReportDto>>>;
