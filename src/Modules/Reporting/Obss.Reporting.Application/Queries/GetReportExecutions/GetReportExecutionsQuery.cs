using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetReportExecutions;

public sealed record GetReportExecutionsQuery(
    Guid ReportDefinitionId) : IRequest<Result<IReadOnlyList<ReportExecutionDto>>>;
