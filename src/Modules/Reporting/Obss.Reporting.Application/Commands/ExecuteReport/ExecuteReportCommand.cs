using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.ExecuteReport;

public sealed record ExecuteReportCommand(
    Guid ReportDefinitionId,
    string ExecutedBy) : IRequest<Result<ReportExecutionDto>>;
