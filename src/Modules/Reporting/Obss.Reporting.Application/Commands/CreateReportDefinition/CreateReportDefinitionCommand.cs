using MediatR;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Commands.CreateReportDefinition;

public sealed record CreateReportDefinitionCommand(
    string TenantId,
    string Name,
    string? Description,
    string ReportType,
    string DataSource,
    string Query,
    string OutputFormat,
    string? Schedule) : IRequest<Result<ReportDefinitionDto>>;
